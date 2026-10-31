<#
  Applies pending SQL migrations to an Azure SQL Database.
  - Migration files live in .\migrations and are named NNN_description.sql (for example 001_create_donations_table.sql).
  - Applied versions are recorded in dbo.SchemaVersions, so each migration runs only once.
  - Each migration runs inside a transaction: if any statement fails, the migration is rolled back and the script stops.
  - A line containing only GO separates batches inside a migration file.
  Works in Windows PowerShell 5.1 (the pipeline agent) without installing any modules.
#>
param(
    [Parameter(Mandatory = $true)][string]$Server,
    [Parameter(Mandatory = $true)][string]$Database,
    [Parameter(Mandatory = $true)][string]$User,
    [Parameter(Mandatory = $true)][string]$Password,
    [string]$MigrationsPath = (Join-Path $PSScriptRoot 'migrations'),
    [string]$Operator = 'manual'
)

$ErrorActionPreference = 'Stop'

function Invoke-Sql {
    param($Connection, [string]$Text, $Transaction = $null)
    $command = $Connection.CreateCommand()
    $command.CommandText = $Text
    $command.CommandTimeout = 120
    if ($Transaction) { $command.Transaction = $Transaction }
    [void]$command.ExecuteNonQuery()
}

$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Data Source'] = 'tcp:' + $Server + ',1433'
$builder['Initial Catalog'] = $Database
$builder['User ID'] = $User
$builder['Password'] = $Password
$builder['Encrypt'] = $true
$builder['Connect Timeout'] = 60

$connection = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)

$connected = $false
for ($attempt = 1; $attempt -le 3; $attempt++) {
    try {
        $connection.Open()
        $connected = $true
        break
    } catch {
        Write-Host ('Connection attempt {0} failed: {1}' -f $attempt, $_.Exception.Message)
        Start-Sleep -Seconds 15
    }
}
if (-not $connected) { throw 'Could not connect to the database after 3 attempts.' }
Write-Host ('Connected to database {0} on {1}' -f $Database, $Server)

try {
    $ensureVersionTable = @'
IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersions (
        Version     NVARCHAR(20)  NOT NULL CONSTRAINT PK_SchemaVersions PRIMARY KEY,
        Description NVARCHAR(200) NOT NULL,
        AppliedAt   DATETIME2     NOT NULL CONSTRAINT DF_SchemaVersions_AppliedAt DEFAULT (SYSUTCDATETIME()),
        AppliedBy   NVARCHAR(100) NOT NULL
    );
END
'@
    Invoke-Sql -Connection $connection -Text $ensureVersionTable

    $applied = @{}
    $listCommand = $connection.CreateCommand()
    $listCommand.CommandText = 'SELECT Version FROM dbo.SchemaVersions'
    $reader = $listCommand.ExecuteReader()
    while ($reader.Read()) { $applied[$reader.GetString(0)] = $true }
    $reader.Close()

    $files = Get-ChildItem -Path $MigrationsPath -Filter '*.sql' | Sort-Object Name
    if (-not $files) { throw ('No migration files found in ' + $MigrationsPath) }

    $appliedNow = 0
    foreach ($file in $files) {
        $parts = $file.BaseName.Split('_', 2)
        $version = $parts[0]
        $description = $file.BaseName
        if ($parts.Length -gt 1) { $description = $parts[1].Replace('_', ' ') }

        if ($applied.ContainsKey($version)) {
            Write-Host ('Skipping migration {0} ({1}): already applied' -f $version, $description)
            continue
        }

        Write-Host ('Applying migration {0} ({1})' -f $version, $description)
        $sql = Get-Content -Path $file.FullName -Raw
        $batches = [regex]::Split($sql, '(?im)^\s*GO\s*$')

        $transaction = $connection.BeginTransaction()
        try {
            foreach ($batch in $batches) {
                if ($batch.Trim().Length -gt 0) {
                    Invoke-Sql -Connection $connection -Text $batch -Transaction $transaction
                }
            }
            $record = $connection.CreateCommand()
            $record.Transaction = $transaction
            $record.CommandText = 'INSERT INTO dbo.SchemaVersions (Version, Description, AppliedBy) VALUES (@v, @d, @b)'
            [void]$record.Parameters.AddWithValue('@v', $version)
            [void]$record.Parameters.AddWithValue('@d', $description)
            [void]$record.Parameters.AddWithValue('@b', $Operator)
            [void]$record.ExecuteNonQuery()
            $transaction.Commit()
            $appliedNow++
            Write-Host ('  migration {0} applied successfully' -f $version)
        } catch {
            $transaction.Rollback()
            throw ('Migration {0} failed and was rolled back: {1}' -f $version, $_.Exception.Message)
        }
    }

    Write-Host ('Migrations applied in this run: {0}' -f $appliedNow)

    $summary = $connection.CreateCommand()
    $summary.CommandText = 'SELECT Version, Description, AppliedAt, AppliedBy FROM dbo.SchemaVersions ORDER BY Version'
    $reader = $summary.ExecuteReader()
    Write-Host 'Schema versions now recorded in the database:'
    while ($reader.Read()) {
        Write-Host ('  {0}  {1}  (applied {2:yyyy-MM-dd HH:mm} UTC by {3})' -f $reader.GetString(0), $reader.GetString(1), $reader.GetDateTime(2), $reader.GetString(3))
    }
    $reader.Close()
}
finally {
    $connection.Close()
}

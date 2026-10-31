-- Rollback for version 003: removes the EmployeeUpdates table and its version record.
IF OBJECT_ID(N'dbo.EmployeeUpdates', N'U') IS NOT NULL
    DROP TABLE dbo.EmployeeUpdates;
GO
DELETE FROM dbo.SchemaVersions WHERE Version = N'003';

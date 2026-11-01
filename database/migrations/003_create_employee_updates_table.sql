-- Version 003: create the EmployeeUpdates table (project updates posted by employees)
IF OBJECT_ID(N'dbo.EmployeeUpdates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeUpdates (
        UpdateId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeUpdates PRIMARY KEY,
        EmployeeId   NVARCHAR(50)      NOT NULL,
        EmployeeName NVARCHAR(100)     NOT NULL,
        Department   NVARCHAR(100)     NULL,
        ProjectName  NVARCHAR(100)     NULL,
        [UpdateText] NVARCHAR(1000)    NOT NULL,
        CreatedAt    DATETIME2         NOT NULL CONSTRAINT DF_EmployeeUpdates_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END
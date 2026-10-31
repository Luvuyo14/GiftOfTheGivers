-- Version 002: create the Volunteers table (volunteer registrations)
IF OBJECT_ID(N'dbo.Volunteers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Volunteers (
        VolunteerId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Volunteers PRIMARY KEY,
        FullName     NVARCHAR(100)     NOT NULL,
        Email        NVARCHAR(200)     NOT NULL,
        Phone        NVARCHAR(30)      NULL,
        Skills       NVARCHAR(500)     NOT NULL,
        Availability NVARCHAR(50)      NOT NULL,
        RegisteredAt DATETIME2         NOT NULL CONSTRAINT DF_Volunteers_RegisteredAt DEFAULT (SYSUTCDATETIME())
    );
END

-- Rollback for version 002: removes the Volunteers table and its version record.
IF OBJECT_ID(N'dbo.Volunteers', N'U') IS NOT NULL
    DROP TABLE dbo.Volunteers;
GO
DELETE FROM dbo.SchemaVersions WHERE Version = N'002';

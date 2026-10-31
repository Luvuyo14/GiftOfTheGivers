-- Rollback for version 001: removes the Donations table and its version record.
-- WARNING: this deletes all donation rows. Use point-in-time restore if data must be kept.
IF OBJECT_ID(N'dbo.Donations', N'U') IS NOT NULL
    DROP TABLE dbo.Donations;
GO
DELETE FROM dbo.SchemaVersions WHERE Version = N'001';

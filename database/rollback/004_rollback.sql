-- Rollback for version 004: removes the index on Donations.CreatedAt and its version record.
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donations_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Donations'))
    DROP INDEX IX_Donations_CreatedAt ON dbo.Donations;
GO
DELETE FROM dbo.SchemaVersions WHERE Version = N'004';

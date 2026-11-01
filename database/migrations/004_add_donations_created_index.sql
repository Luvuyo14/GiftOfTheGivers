-- Version 004: speed up reports that list donations by date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Donations_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Donations'))
BEGIN
    CREATE INDEX IX_Donations_CreatedAt ON dbo.Donations (CreatedAt);
END
-- Version 001: create the Donations table (one row per donation and tax certificate)
IF OBJECT_ID(N'dbo.Donations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Donations (
        DonationId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Donations PRIMARY KEY,
        DonorName         NVARCHAR(100)     NOT NULL,
        Email             NVARCHAR(200)     NULL,
        Amount            DECIMAL(18,2)     NOT NULL,
        Currency          NCHAR(3)          NOT NULL CONSTRAINT DF_Donations_Currency DEFAULT ('ZAR'),
        DonationType      NVARCHAR(20)      NOT NULL,
        CertificateNumber NVARCHAR(40)      NULL,
        CreatedAt         DATETIME2         NOT NULL CONSTRAINT DF_Donations_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END

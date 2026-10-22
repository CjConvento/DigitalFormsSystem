-- ============================================================================
-- Digital Forms System - Automated Database Setup
-- Target: Microsoft SQL Server
-- Instructions: Execute this entire script in SQL Server Management Studio (SSMS) 
--               while logged in as an Administrator (e.g., 'sa').
-- ============================================================================

-- 0. Create Database (If it doesn't exist yet)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DigitalFormsSystem')
BEGIN
    CREATE DATABASE [DigitalFormsSystem];
    PRINT '✅ Database [DigitalFormsSystem] created successfully.';
END
ELSE
BEGIN
    PRINT 'ℹ️ Database [DigitalFormsSystem] already exists. Skipping creation.';
END
GO

-- Switch context to the DigitalFormsSystem database
USE [DigitalFormsSystem];
GO

-- ============================================================================
-- 1. Employees Table (Core Reference)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Employees' AND xtype='U')
BEGIN
    CREATE TABLE Employees (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeNo NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(255) NOT NULL,
        DateHired DATETIME2 NULL,                             
        Company NVARCHAR(100),
        Location NVARCHAR(100),
        Department NVARCHAR(100),
        [Section] NVARCHAR(100),
        Category NVARCHAR(50),
        [Status] NVARCHAR(50) DEFAULT 'Active',
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETDATE(),               
        PasswordHash NVARCHAR(255) NULL,
        IsFirstLogin BIT DEFAULT 1                         
    );
    CREATE INDEX IX_Employees_EmployeeNo ON Employees(EmployeeNo);
    CREATE INDEX IX_Employees_IsActive ON Employees(IsActive);
    PRINT '✅ Table [Employees] created.';
END
GO

-- ============================================================================
-- 2. Damaged Reports Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DamagedReports' AND xtype='U')
BEGIN
    CREATE TABLE DamagedReports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ControlNo NVARCHAR(50) NOT NULL UNIQUE,
        Item NVARCHAR(255) NOT NULL,
        CauseOfDamage NVARCHAR(MAX),
        FixedAssetCode NVARCHAR(100),
        DatePurchased DATE,
        BrandSize NVARCHAR(100),
        LocationUser NVARCHAR(255),
        SerialNumber NVARCHAR(100),
        Color NVARCHAR(50),
        IncidentDateTime DATETIME2,
        ImmediateAction NVARCHAR(MAX),
        RecommendedAction NVARCHAR(MAX),
        Remarks NVARCHAR(MAX),
        ReportedByEmployeeId INT NOT NULL,
        ReceivedByEmployeeId INT,
        ReceivedDateTime DATETIME2,
        RequestStatus NVARCHAR(50) DEFAULT 'Draft',
        Findings NVARCHAR(MAX),
        Recommendation NVARCHAR(MAX),
        NegligenceFlag BIT DEFAULT 0,
        NegligenceDetails NVARCHAR(MAX),
        AdministrativeDiscipline NVARCHAR(MAX),
        InvestigatedByEmployeeId INT,
        VerifiedByEmployeeId INT,
        NotedByEmployeeId INT,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        
        CONSTRAINT FK_DamagedReports_ReportedBy FOREIGN KEY (ReportedByEmployeeId) REFERENCES Employees(ID),
        CONSTRAINT FK_DamagedReports_ReceivedBy FOREIGN KEY (ReceivedByEmployeeId) REFERENCES Employees(ID)
    );
    CREATE INDEX IX_DamagedReports_ControlNo ON DamagedReports(ControlNo);
    CREATE INDEX IX_DamagedReports_ReportedBy ON DamagedReports(ReportedByEmployeeId);
    CREATE INDEX IX_DamagedReports_Status ON DamagedReports(RequestStatus);
    PRINT '✅ Table [DamagedReports] created.';
END
GO

-- ============================================================================
-- 3. Damaged Report Images Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DamagedReportImages' AND xtype='U')
BEGIN
    CREATE TABLE DamagedReportImages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DamagedReportId INT NOT NULL,
        [Section] NVARCHAR(50) NOT NULL, -- e.g., 'PartI', 'PartII'
        FileName NVARCHAR(255) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        StorageFileId NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100),
        UploadedAt DATETIME2 DEFAULT GETDATE(),

        CONSTRAINT FK_DamagedReportImages_Report FOREIGN KEY (DamagedReportId) REFERENCES DamagedReports(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_DamagedReportImages_ReportId ON DamagedReportImages(DamagedReportId);
    PRINT '✅ Table [DamagedReportImages] created.';
END
GO

-- ============================================================================
-- 4. Damaged Report Follow-Ups Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DamagedReportFollowUps' AND xtype='U')
BEGIN
    CREATE TABLE DamagedReportFollowUps (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DamagedReportId INT NOT NULL,
        FollowUpDate DATETIME2 NOT NULL,
        [Status] NVARCHAR(100) NOT NULL,
        UpdateBy NVARCHAR(255),
        NotedBy NVARCHAR(255),
        CreatedAt DATETIME2 DEFAULT GETDATE(),

        CONSTRAINT FK_DamagedReportFollowUps_Report FOREIGN KEY (DamagedReportId) REFERENCES DamagedReports(Id) ON DELETE CASCADE
    );
    PRINT '✅ Table [DamagedReportFollowUps] created.';
END
GO

-- ============================================================================
-- 5. Fixed Asset Requests Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FixedAssetRequests' AND xtype='U')
BEGIN
    CREATE TABLE FixedAssetRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ControlNo NVARCHAR(50) NOT NULL UNIQUE,
        DateRequested DATE NOT NULL,
        Department NVARCHAR(100),
        TargetDateNeeded DATE,
        [Section] NVARCHAR(100),
        Quantity INT DEFAULT 1,
        AssetTypeId INT,
        AssetType NVARCHAR(255),
        DetailedDescription NVARCHAR(MAX),
        ReasonPurpose NVARCHAR(MAX),
        ProposedLocation NVARCHAR(255),
        EstimatedLifeSpan NVARCHAR(100),
        RequestType NVARCHAR(100),
        ExistingUnitCount INT DEFAULT 0,
        ExistingUser NVARCHAR(255),
        DamagedReportNo NVARCHAR(50),
        EvaluatedByName NVARCHAR(255),
        RequestedByName NVARCHAR(255),
        RequestedByEmployeeId INT NOT NULL,
        EvaluatedByEmployeeId INT,
        RequestStatus NVARCHAR(50) DEFAULT 'Draft',
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        RequestedAt DATETIME2 DEFAULT GETDATE(),

        CONSTRAINT FK_FixedAssetRequests_RequestedBy FOREIGN KEY (RequestedByEmployeeId) REFERENCES Employees(Id),
        CONSTRAINT FK_FixedAssetRequests_EvaluatedBy FOREIGN KEY (EvaluatedByEmployeeId) REFERENCES Employees(Id)
    );
    CREATE INDEX IX_FixedAssetRequests_ControlNo ON FixedAssetRequests(ControlNo);
    CREATE INDEX IX_FixedAssetRequests_RequestedBy ON FixedAssetRequests(RequestedByEmployeeId);
    CREATE INDEX IX_FixedAssetRequests_Status ON FixedAssetRequests(RequestStatus);
    PRINT '✅ Table [FixedAssetRequests] created.';
END
GO

-- ============================================================================
-- 6. Existing Unit Details Table (Nested in Fixed Asset Requests)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ExistingUnitDetails' AND xtype='U')
BEGIN
    CREATE TABLE ExistingUnitDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FixedAssetRequestId INT NOT NULL,
        ItemNo INT NOT NULL,
        Description NVARCHAR(MAX),
        Location NVARCHAR(255),
        UserName NVARCHAR(255),
        Remarks NVARCHAR(MAX),
        CreatedAt DATETIME2 DEFAULT GETDATE(),

        CONSTRAINT FK_ExistingUnitDetails_Request FOREIGN KEY (FixedAssetRequestId) REFERENCES FixedAssetRequests(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ExistingUnitDetails_RequestId ON ExistingUnitDetails(FixedAssetRequestId);
    PRINT '✅ Table [ExistingUnitDetails] created.';
END
GO

PRINT '🎉 DFS Schema and Seed Data successfully applied!';
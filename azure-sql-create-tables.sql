-- Azure SQL Database Table Creation Script
-- This script creates all tables and the EF Migrations history table
-- Run this in SQL Server Management Studio connected to your Azure SQL Database

-- Create the EF Migrations History table first
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Create SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SubscriptionPlans' AND xtype='U')
BEGIN
    CREATE TABLE [SubscriptionPlans] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [MonthlyPriceAmount] decimal(18,2) NOT NULL,
        [MonthlyPriceCurrency] nvarchar(3) NOT NULL,
        [YearlyPriceAmount] decimal(18,2) NOT NULL,
        [YearlyPriceCurrency] nvarchar(3) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [MaxLocations] int NOT NULL DEFAULT 1,
        [MaxStaffPerLocation] int NOT NULL DEFAULT 5,
        [IncludesAnalytics] bit NOT NULL DEFAULT 0,
        [IncludesAdvancedReporting] bit NOT NULL DEFAULT 0,
        [IncludesCustomBranding] bit NOT NULL DEFAULT 0,
        [IncludesAdvertising] bit NOT NULL DEFAULT 0,
        [IncludesMultipleLocations] bit NOT NULL DEFAULT 0,
        [MaxQueueEntriesPerDay] int NOT NULL DEFAULT 100,
        [IsFeatured] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_SubscriptionPlans_Name] ON [SubscriptionPlans] ([Name]);
    CREATE INDEX [IX_SubscriptionPlans_IsActive] ON [SubscriptionPlans] ([IsActive]);
    CREATE INDEX [IX_SubscriptionPlans_IsDefault] ON [SubscriptionPlans] ([IsDefault]);
    CREATE INDEX [IX_SubscriptionPlans_IsFeatured] ON [SubscriptionPlans] ([IsFeatured]);
    CREATE INDEX [IX_SubscriptionPlans_Price] ON [SubscriptionPlans] ([Price]);
END

-- Create Organizations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Organizations' AND xtype='U')
BEGIN
    CREATE TABLE [Organizations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [ContactEmail] nvarchar(320) NULL,
        [ContactPhone] nvarchar(50) NULL,
        [WebsiteUrl] nvarchar(500) NULL,
        [BrandingPrimaryColor] nvarchar(50) NOT NULL,
        [BrandingSecondaryColor] nvarchar(50) NOT NULL,
        [BrandingLogoUrl] nvarchar(500) NOT NULL,
        [BrandingFaviconUrl] nvarchar(500) NOT NULL,
        [BrandingCompanyName] nvarchar(200) NOT NULL,
        [BrandingTagLine] nvarchar(500) NOT NULL,
        [BrandingFontFamily] nvarchar(100) NOT NULL,
        [SubscriptionPlanId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [SharesDataForAnalytics] bit NOT NULL DEFAULT 0,
        [LocationIds] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Organizations_Slug] ON [Organizations] ([Slug]);
    CREATE INDEX [IX_Organizations_ContactEmail] ON [Organizations] ([ContactEmail]);
    CREATE INDEX [IX_Organizations_IsActive] ON [Organizations] ([IsActive]);
    CREATE INDEX [IX_Organizations_Name] ON [Organizations] ([Name]);
    CREATE INDEX [IX_Organizations_SubscriptionPlanId] ON [Organizations] ([SubscriptionPlanId]);
END

-- Create Locations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Locations' AND xtype='U')
BEGIN
    CREATE TABLE [Locations] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [AddressStreet] nvarchar(200) NOT NULL,
        [AddressNumber] nvarchar(20) NOT NULL,
        [AddressComplement] nvarchar(100) NOT NULL,
        [AddressNeighborhood] nvarchar(100) NOT NULL,
        [AddressCity] nvarchar(100) NOT NULL,
        [AddressState] nvarchar(50) NOT NULL,
        [AddressCountry] nvarchar(50) NOT NULL,
        [AddressPostalCode] nvarchar(20) NOT NULL,
        [AddressLatitude] float NULL,
        [AddressLongitude] float NULL,
        [ContactPhone] nvarchar(50) NULL,
        [ContactEmail] nvarchar(320) NULL,
        [CustomBrandingPrimaryColor] nvarchar(50) NULL,
        [CustomBrandingSecondaryColor] nvarchar(50) NULL,
        [CustomBrandingLogoUrl] nvarchar(500) NULL,
        [CustomBrandingFaviconUrl] nvarchar(500) NULL,
        [CustomBrandingCompanyName] nvarchar(200) NULL,
        [CustomBrandingTagLine] nvarchar(500) NULL,
        [CustomBrandingFontFamily] nvarchar(100) NULL,
        [BusinessHoursStart] time NOT NULL,
        [BusinessHoursEnd] time NOT NULL,
        [IsQueueEnabled] bit NOT NULL DEFAULT 1,
        [MaxQueueSize] int NOT NULL DEFAULT 100,
        [LateClientCapTimeInMinutes] int NOT NULL DEFAULT 15,
        [IsActive] bit NOT NULL DEFAULT 1,
        [AverageServiceTimeInMinutes] float NOT NULL DEFAULT 30.0,
        [LastAverageTimeReset] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [StaffMemberIds] nvarchar(max) NOT NULL,
        [ServiceTypeIds] nvarchar(max) NOT NULL,
        [AdvertisementIds] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Locations_Slug] ON [Locations] ([Slug]);
    CREATE INDEX [IX_Locations_ContactEmail] ON [Locations] ([ContactEmail]);
    CREATE INDEX [IX_Locations_IsActive] ON [Locations] ([IsActive]);
    CREATE INDEX [IX_Locations_IsQueueEnabled] ON [Locations] ([IsQueueEnabled]);
    CREATE INDEX [IX_Locations_Name] ON [Locations] ([Name]);
    CREATE INDEX [IX_Locations_OrganizationId] ON [Locations] ([OrganizationId]);
END

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [IsLocked] bit NOT NULL DEFAULT 0,
        [RequiresTwoFactor] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END

-- Create Customers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
BEGIN
    CREATE TABLE [Customers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(50) NULL,
        [Email] nvarchar(320) NULL,
        [IsAnonymous] bit NOT NULL DEFAULT 0,
        [NotificationsEnabled] bit NOT NULL DEFAULT 1,
        [PreferredNotificationChannel] nvarchar(20) NULL,
        [UserId] nvarchar(50) NULL,
        [FavoriteLocationIds] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_Customers_Email] ON [Customers] ([Email]);
    CREATE INDEX [IX_Customers_IsAnonymous] ON [Customers] ([IsAnonymous]);
    CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name]);
    CREATE INDEX [IX_Customers_PhoneNumber] ON [Customers] ([PhoneNumber]);
    CREATE INDEX [IX_Customers_UserId] ON [Customers] ([UserId]);
END

-- Create ServicesOffered table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ServicesOffered' AND xtype='U')
BEGIN
    CREATE TABLE [ServicesOffered] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [EstimatedDurationMinutes] int NOT NULL DEFAULT 30,
        [PriceAmount] decimal(18,2) NULL,
        [PriceCurrency] nvarchar(3) NULL,
        [ImageUrl] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [TimesProvided] int NOT NULL DEFAULT 0,
        [ActualAverageDurationMinutes] float NOT NULL DEFAULT 30.0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_ServicesOffered] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_ServicesOffered_EstimatedDuration] ON [ServicesOffered] ([EstimatedDurationMinutes]);
    CREATE INDEX [IX_ServicesOffered_IsActive] ON [ServicesOffered] ([IsActive]);
    CREATE INDEX [IX_ServicesOffered_Location_Active] ON [ServicesOffered] ([LocationId], [IsActive]);
    CREATE INDEX [IX_ServicesOffered_LocationId] ON [ServicesOffered] ([LocationId]);
    CREATE INDEX [IX_ServicesOffered_Name] ON [ServicesOffered] ([Name]);
END

-- Create StaffMembers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StaffMembers' AND xtype='U')
BEGIN
    CREATE TABLE [StaffMembers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [Email] nvarchar(320) NULL,
        [PhoneNumber] nvarchar(50) NULL,
        [ProfilePictureUrl] nvarchar(500) NULL,
        [Role] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [IsOnDuty] bit NOT NULL DEFAULT 0,
        [StaffStatus] nvarchar(20) NOT NULL DEFAULT 'available',
        [UserId] nvarchar(50) NULL,
        [AverageServiceTimeInMinutes] float NOT NULL DEFAULT 30.0,
        [CompletedServicesCount] int NOT NULL DEFAULT 0,
        [EmployeeCode] nvarchar(20) NOT NULL,
        [Username] nvarchar(50) NOT NULL,
        [SpecialtyServiceTypeIds] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] varbinary(max) NOT NULL DEFAULT 0x,
        CONSTRAINT [PK_StaffMembers] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_StaffMembers_EmployeeCode] ON [StaffMembers] ([EmployeeCode]);
    CREATE UNIQUE INDEX [IX_StaffMembers_Username] ON [StaffMembers] ([Username]);
    CREATE INDEX [IX_StaffMembers_Email] ON [StaffMembers] ([Email]);
    CREATE INDEX [IX_StaffMembers_IsActive] ON [StaffMembers] ([IsActive]);
    CREATE INDEX [IX_StaffMembers_LocationId] ON [StaffMembers] ([LocationId]);
    CREATE INDEX [IX_StaffMembers_Name] ON [StaffMembers] ([Name]);
    CREATE INDEX [IX_StaffMembers_PhoneNumber] ON [StaffMembers] ([PhoneNumber]);
    CREATE INDEX [IX_StaffMembers_UserId] ON [StaffMembers] ([UserId]);
END

-- Create Queues table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Queues' AND xtype='U')
BEGIN
    CREATE TABLE [Queues] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [QueueDate] date NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [MaxSize] int NOT NULL DEFAULT 100,
        [LateClientCapTimeInMinutes] int NOT NULL DEFAULT 15,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Queues] PRIMARY KEY ([Id])
    );
END

-- Create QueueEntries table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QueueEntries' AND xtype='U')
BEGIN
    CREATE TABLE [QueueEntries] (
        [Id] uniqueidentifier NOT NULL,
        [QueueId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [Position] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [StaffMemberId] uniqueidentifier NULL,
        [ServiceTypeId] uniqueidentifier NULL,
        [Notes] nvarchar(1000) NULL,
        [EnteredAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CalledAt] datetime2 NULL,
        [CheckedInAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        [CancelledAt] datetime2 NULL,
        [ServiceDurationMinutes] int NULL,
        [EstimatedDurationMinutes] int NULL,
        [EstimatedStartTime] datetime2 NULL,
        [ActualStartTime] datetime2 NULL,
        [CompletionTime] datetime2 NULL,
        [TokenNumber] int NOT NULL,
        [NotificationSent] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_QueueEntries] PRIMARY KEY ([Id])
    );
END

-- Create CustomerServiceHistory table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CustomerServiceHistory' AND xtype='U')
BEGIN
    CREATE TABLE [CustomerServiceHistory] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [StaffMemberId] uniqueidentifier NOT NULL,
        [ServiceTypeId] uniqueidentifier NOT NULL,
        [ServiceDate] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [Rating] int NULL,
        [Feedback] nvarchar(2000) NULL,
        [CustomerId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_CustomerServiceHistory] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_CustomerServiceHistory_CustomerId] ON [CustomerServiceHistory] ([CustomerId]);
END

-- Insert migration history records
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250726170309_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250726170309_InitialCreate', '8.0.0');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250726193250_AddUserAndQueues')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250726193250_AddUserAndQueues', '8.0.0');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250727183900_AddOrganizationLocationCustomer')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250727183900_AddOrganizationLocationCustomer', '8.0.0');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250728015448_AddStaffSubscriptionServiceNoBreaks')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250728015448_AddStaffSubscriptionServiceNoBreaks', '8.0.0');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250802185542_AddConcurrencyTokens')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250802185542_AddConcurrencyTokens', '8.0.0');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250804014627_RefactorToJsonColumn')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250804014627_RefactorToJsonColumn', '8.0.0');
END

PRINT 'All tables and migration history created successfully!';

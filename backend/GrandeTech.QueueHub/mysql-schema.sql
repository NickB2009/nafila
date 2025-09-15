-- MySQL Database Creation Script
-- GrandeTech QueueHub Database
-- Migration from SQL Server to MySQL

CREATE DATABASE IF NOT EXISTS QueueHubDb 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE QueueHubDb;

-- Create EF Migrations History table
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

-- Create SubscriptionPlans table
CREATE TABLE IF NOT EXISTS `SubscriptionPlans` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Description` TEXT NOT NULL,
    `MonthlyPriceAmount` DECIMAL(18,2) NOT NULL,
    `MonthlyPriceCurrency` VARCHAR(3) NOT NULL,
    `YearlyPriceAmount` DECIMAL(18,2) NOT NULL,
    `YearlyPriceCurrency` VARCHAR(3) NOT NULL,
    `Price` DECIMAL(18,2) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `MaxLocations` INT NOT NULL DEFAULT 1,
    `MaxStaffPerLocation` INT NOT NULL DEFAULT 5,
    `IncludesAnalytics` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesAdvancedReporting` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesCustomBranding` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesAdvertising` TINYINT(1) NOT NULL DEFAULT 0,
    `IncludesMultipleLocations` TINYINT(1) NOT NULL DEFAULT 0,
    `MaxQueueEntriesPerDay` INT NOT NULL DEFAULT 100,
    `IsFeatured` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_SubscriptionPlans_Name` (`Name`),
    KEY `IX_SubscriptionPlans_IsActive` (`IsActive`),
    KEY `IX_SubscriptionPlans_IsDefault` (`IsDefault`),
    KEY `IX_SubscriptionPlans_IsFeatured` (`IsFeatured`),
    KEY `IX_SubscriptionPlans_Price` (`Price`)
);

-- Create Organizations table
CREATE TABLE IF NOT EXISTS `Organizations` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `Description` TEXT NULL,
    `ContactEmail` VARCHAR(320) NULL,
    `ContactPhone` VARCHAR(50) NULL,
    `WebsiteUrl` VARCHAR(500) NULL,
    `BrandingPrimaryColor` VARCHAR(50) NOT NULL,
    `BrandingSecondaryColor` VARCHAR(50) NOT NULL,
    `BrandingLogoUrl` VARCHAR(500) NOT NULL,
    `BrandingFaviconUrl` VARCHAR(500) NOT NULL,
    `BrandingCompanyName` VARCHAR(200) NOT NULL,
    `BrandingTagLine` VARCHAR(500) NOT NULL,
    `BrandingFontFamily` VARCHAR(100) NOT NULL,
    `SubscriptionPlanId` CHAR(36) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `SharesDataForAnalytics` TINYINT(1) NOT NULL DEFAULT 0,
    `LocationIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Organizations_Slug` (`Slug`),
    KEY `IX_Organizations_ContactEmail` (`ContactEmail`),
    KEY `IX_Organizations_IsActive` (`IsActive`),
    KEY `IX_Organizations_Name` (`Name`),
    KEY `IX_Organizations_SubscriptionPlanId` (`SubscriptionPlanId`)
);

-- Create Locations table
CREATE TABLE IF NOT EXISTS `Locations` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `Description` TEXT NULL,
    `OrganizationId` CHAR(36) NOT NULL,
    `AddressStreet` VARCHAR(200) NOT NULL,
    `AddressNumber` VARCHAR(20) NOT NULL,
    `AddressComplement` VARCHAR(100) NOT NULL,
    `AddressNeighborhood` VARCHAR(100) NOT NULL,
    `AddressCity` VARCHAR(100) NOT NULL,
    `AddressState` VARCHAR(50) NOT NULL,
    `AddressCountry` VARCHAR(50) NOT NULL,
    `AddressPostalCode` VARCHAR(20) NOT NULL,
    `AddressLatitude` DOUBLE NULL,
    `AddressLongitude` DOUBLE NULL,
    `ContactPhone` VARCHAR(50) NULL,
    `ContactEmail` VARCHAR(320) NULL,
    `CustomBrandingPrimaryColor` VARCHAR(50) NULL,
    `CustomBrandingSecondaryColor` VARCHAR(50) NULL,
    `CustomBrandingLogoUrl` VARCHAR(500) NULL,
    `CustomBrandingFaviconUrl` VARCHAR(500) NULL,
    `CustomBrandingCompanyName` VARCHAR(200) NULL,
    `CustomBrandingTagLine` VARCHAR(500) NULL,
    `CustomBrandingFontFamily` VARCHAR(100) NULL,
    `BusinessHoursStart` TIME NOT NULL,
    `BusinessHoursEnd` TIME NOT NULL,
    `IsQueueEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `MaxQueueSize` INT NOT NULL DEFAULT 100,
    `LateClientCapTimeInMinutes` INT NOT NULL DEFAULT 15,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `AverageServiceTimeInMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `LastAverageTimeReset` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `StaffMemberIds` JSON NOT NULL,
    `ServiceTypeIds` JSON NOT NULL,
    `AdvertisementIds` JSON NOT NULL,
    `WeeklyBusinessHours` LONGTEXT NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Locations_Slug` (`Slug`),
    KEY `IX_Locations_ContactEmail` (`ContactEmail`),
    KEY `IX_Locations_IsActive` (`IsActive`),
    KEY `IX_Locations_IsQueueEnabled` (`IsQueueEnabled`),
    KEY `IX_Locations_Name` (`Name`),
    KEY `IX_Locations_OrganizationId` (`OrganizationId`)
);

-- Create Users table
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL,
    `FullName` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(255) NOT NULL,
    `PhoneNumber` VARCHAR(20) NOT NULL,
    `PasswordHash` VARCHAR(500) NOT NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `LastLoginAt` DATETIME(6) NULL,
    `IsLocked` TINYINT(1) NOT NULL DEFAULT 0,
    `RequiresTwoFactor` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_Users_Email` (`Email`),
    KEY `IX_Users_IsActive` (`IsActive`),
    KEY `IX_Users_Role` (`Role`)
);

-- Create Customers table
CREATE TABLE IF NOT EXISTS `Customers` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `PhoneNumber` VARCHAR(50) NULL,
    `Email` VARCHAR(320) NULL,
    `IsAnonymous` TINYINT(1) NOT NULL DEFAULT 0,
    `NotificationsEnabled` TINYINT(1) NOT NULL DEFAULT 1,
    `PreferredNotificationChannel` VARCHAR(20) NULL,
    `UserId` VARCHAR(50) NULL,
    `FavoriteLocationIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_Customers_Email` (`Email`),
    KEY `IX_Customers_IsAnonymous` (`IsAnonymous`),
    KEY `IX_Customers_Name` (`Name`),
    KEY `IX_Customers_PhoneNumber` (`PhoneNumber`),
    KEY `IX_Customers_UserId` (`UserId`)
);

-- Create ServicesOffered table
CREATE TABLE IF NOT EXISTS `ServicesOffered` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `Description` TEXT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `EstimatedDurationMinutes` INT NOT NULL DEFAULT 30,
    `PriceAmount` DECIMAL(18,2) NULL,
    `PriceCurrency` VARCHAR(3) NULL,
    `ImageUrl` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `TimesProvided` INT NOT NULL DEFAULT 0,
    `ActualAverageDurationMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_ServicesOffered_EstimatedDuration` (`EstimatedDurationMinutes`),
    KEY `IX_ServicesOffered_IsActive` (`IsActive`),
    KEY `IX_ServicesOffered_Location_Active` (`LocationId`, `IsActive`),
    KEY `IX_ServicesOffered_LocationId` (`LocationId`),
    KEY `IX_ServicesOffered_Name` (`Name`)
);

-- Create StaffMembers table
CREATE TABLE IF NOT EXISTS `StaffMembers` (
    `Id` CHAR(36) NOT NULL,
    `Name` VARCHAR(200) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `Email` VARCHAR(320) NULL,
    `PhoneNumber` VARCHAR(50) NULL,
    `ProfilePictureUrl` VARCHAR(500) NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsOnDuty` TINYINT(1) NOT NULL DEFAULT 0,
    `StaffStatus` VARCHAR(20) NOT NULL DEFAULT 'available',
    `UserId` VARCHAR(50) NULL,
    `AverageServiceTimeInMinutes` DOUBLE NOT NULL DEFAULT 30.0,
    `CompletedServicesCount` INT NOT NULL DEFAULT 0,
    `EmployeeCode` VARCHAR(20) NOT NULL,
    `Username` VARCHAR(50) NOT NULL,
    `SpecialtyServiceTypeIds` JSON NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_StaffMembers_EmployeeCode` (`EmployeeCode`),
    UNIQUE KEY `IX_StaffMembers_Username` (`Username`),
    KEY `IX_StaffMembers_Email` (`Email`),
    KEY `IX_StaffMembers_IsActive` (`IsActive`),
    KEY `IX_StaffMembers_LocationId` (`LocationId`),
    KEY `IX_StaffMembers_Name` (`Name`),
    KEY `IX_StaffMembers_PhoneNumber` (`PhoneNumber`),
    KEY `IX_StaffMembers_UserId` (`UserId`)
);

-- Create Queues table
CREATE TABLE IF NOT EXISTS `Queues` (
    `Id` CHAR(36) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `QueueDate` DATE NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `MaxSize` INT NOT NULL DEFAULT 100,
    `LateClientCapTimeInMinutes` INT NOT NULL DEFAULT 15,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_Queues_LocationId` (`LocationId`),
    KEY `IX_Queues_QueueDate` (`QueueDate`),
    KEY `IX_Queues_IsActive` (`IsActive`)
);

-- Create QueueEntries table
CREATE TABLE IF NOT EXISTS `QueueEntries` (
    `Id` CHAR(36) NOT NULL,
    `QueueId` CHAR(36) NOT NULL,
    `CustomerId` CHAR(36) NOT NULL,
    `CustomerName` VARCHAR(200) NOT NULL,
    `Position` INT NOT NULL,
    `Status` VARCHAR(20) NOT NULL,
    `StaffMemberId` CHAR(36) NULL,
    `ServiceTypeId` CHAR(36) NULL,
    `Notes` TEXT NULL,
    `EnteredAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CalledAt` DATETIME(6) NULL,
    `CheckedInAt` DATETIME(6) NULL,
    `CompletedAt` DATETIME(6) NULL,
    `CancelledAt` DATETIME(6) NULL,
    `ServiceDurationMinutes` INT NULL,
    `EstimatedDurationMinutes` INT NULL,
    `EstimatedStartTime` DATETIME(6) NULL,
    `ActualStartTime` DATETIME(6) NULL,
    `CompletionTime` DATETIME(6) NULL,
    `CustomerNotes` VARCHAR(500) NULL,
    `StaffNotes` VARCHAR(500) NULL,
    `TokenNumber` VARCHAR(20) NOT NULL,
    `NotificationSent` TINYINT(1) NOT NULL DEFAULT 0,
    `NotificationSentAt` DATETIME(6) NULL,
    `NotificationChannel` VARCHAR(20) NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    KEY `IX_QueueEntries_QueueId` (`QueueId`),
    KEY `IX_QueueEntries_CustomerId` (`CustomerId`),
    KEY `IX_QueueEntries_Status` (`Status`),
    KEY `IX_QueueEntries_Position` (`Position`),
    KEY `IX_QueueEntries_EnteredAt` (`EnteredAt`)
);

-- Create CustomerServiceHistory table
CREATE TABLE IF NOT EXISTS `CustomerServiceHistory` (
    `Id` CHAR(36) NOT NULL,
    `CustomerId` CHAR(36) NOT NULL,
    `LocationId` CHAR(36) NOT NULL,
    `StaffMemberId` CHAR(36) NOT NULL,
    `ServiceTypeId` CHAR(36) NOT NULL,
    `ServiceDate` DATETIME(6) NOT NULL,
    `Notes` TEXT NULL,
    `Rating` INT NULL,
    `Feedback` TEXT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_CustomerServiceHistory_CustomerId` (`CustomerId`),
    KEY `IX_CustomerServiceHistory_LocationId` (`LocationId`),
    KEY `IX_CustomerServiceHistory_ServiceDate` (`ServiceDate`)
);

-- Insert migration history records
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES 
('20250726170309_InitialCreate', '8.0.0'),
('20250726193250_AddUserAndQueues', '8.0.0'),
('20250727183900_AddOrganizationLocationCustomer', '8.0.0'),
('20250728015448_AddStaffSubscriptionServiceNoBreaks', '8.0.0'),
('20250802185542_AddConcurrencyTokens', '8.0.0'),
('20250804014627_RefactorToJsonColumn', '8.0.0');

-- Add foreign key constraints
ALTER TABLE `Organizations` 
ADD CONSTRAINT `FK_Organizations_SubscriptionPlans_SubscriptionPlanId` 
FOREIGN KEY (`SubscriptionPlanId`) REFERENCES `SubscriptionPlans` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `Locations` 
ADD CONSTRAINT `FK_Locations_Organizations_OrganizationId` 
FOREIGN KEY (`OrganizationId`) REFERENCES `Organizations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `ServicesOffered` 
ADD CONSTRAINT `FK_ServicesOffered_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `StaffMembers` 
ADD CONSTRAINT `FK_StaffMembers_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `Queues` 
ADD CONSTRAINT `FK_Queues_Locations_LocationId` 
FOREIGN KEY (`LocationId`) REFERENCES `Locations` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Queues_QueueId` 
FOREIGN KEY (`QueueId`) REFERENCES `Queues` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;

ALTER TABLE `QueueEntries` 
ADD CONSTRAINT `FK_QueueEntries_StaffMembers_StaffMemberId` 
FOREIGN KEY (`StaffMemberId`) REFERENCES `StaffMembers` (`Id`) ON DELETE SET NULL;

ALTER TABLE `CustomerServiceHistory` 
ADD CONSTRAINT `FK_CustomerServiceHistory_Customers_CustomerId` 
FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT;

-- Note: All indexes already created in table definitions above
-- No additional indexes needed - avoiding duplicates

-- Add JSON indexes for better performance (MySQL 8.0+)
-- Note: These will be created when the JSON columns contain data
-- CREATE INDEX `IX_Organizations_LocationIds` ON `Organizations`((CAST(`LocationIds` AS CHAR(255) ARRAY)));
-- CREATE INDEX `IX_Locations_StaffMemberIds` ON `Locations`((CAST(`StaffMemberIds` AS CHAR(255) ARRAY)));

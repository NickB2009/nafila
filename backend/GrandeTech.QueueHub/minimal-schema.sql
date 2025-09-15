-- Minimal MySQL Schema for QueueHub
-- Core tables only for testing

USE QueueHubDb;

-- Create EF Migrations History table
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

-- Create SubscriptionPlans table (required first due to FK)
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
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`)
);

-- Create Users table
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL,
    `FullName` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(100) NOT NULL,
    `PhoneNumber` VARCHAR(20) NOT NULL,
    `PasswordHash` LONGTEXT NOT NULL,
    `Role` LONGTEXT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL,
    `LastLoginAt` DATETIME(6) NULL,
    `IsLocked` TINYINT(1) NOT NULL,
    `RequiresTwoFactor` TINYINT(1) NOT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `CreatedBy` LONGTEXT NULL,
    `LastModifiedAt` DATETIME(6) NULL,
    `LastModifiedBy` LONGTEXT NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    `DeletedAt` DATETIME(6) NULL,
    `DeletedBy` LONGTEXT NULL,
    `RowVersion` LONGBLOB NOT NULL,
    PRIMARY KEY (`Id`)
);

-- Add some basic seed data
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
('20250112000000_InitialMySqlMigration', '8.0.11');

-- Add a default subscription plan
INSERT IGNORE INTO `SubscriptionPlans` (
    `Id`, `Name`, `Description`, `MonthlyPriceAmount`, `MonthlyPriceCurrency`,
    `YearlyPriceAmount`, `YearlyPriceCurrency`, `Price`, `IsActive`, `IsDefault`,
    `CreatedAt`, `RowVersion`
) VALUES (
    '550e8400-e29b-41d4-a716-446655440000', 
    'Basic Plan', 
    'Basic queue management features',
    29.99, 'USD', 299.99, 'USD', 29.99, 1, 1,
    NOW(), 0x0000000000000000
);

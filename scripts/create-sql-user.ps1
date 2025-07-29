#!/usr/bin/env pwsh
# Create SQL login and user for Azure SQL Database

Write-Host "🔧 Creating SQL login and user for Azure SQL Database..." -ForegroundColor Green

# SQL script to create login and user
$sqlScript = @"
-- Create SQL login
CREATE LOGIN sqladmin WITH PASSWORD = 'YourStrongPassword123!';

-- Create database user
USE GrandeTechQueueHub;
CREATE USER sqladmin FOR LOGIN sqladmin;

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER sqladmin;
GRANT CONNECT TO sqladmin;
GRANT CREATE TABLE TO sqladmin;
GRANT CREATE VIEW TO sqladmin;
GRANT CREATE PROCEDURE TO sqladmin;
GRANT CREATE FUNCTION TO sqladmin;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO sqladmin;
"@

Write-Host "📝 SQL Script to execute:" -ForegroundColor Yellow
Write-Host $sqlScript -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Instructions:" -ForegroundColor Cyan
Write-Host "1. Connect to Azure SQL Database using SQL Server Management Studio" -ForegroundColor White
Write-Host "2. Connect as Azure AD admin (your account)" -ForegroundColor White
Write-Host "3. Execute the above SQL script" -ForegroundColor White
Write-Host "4. Update your connection string with the new credentials" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Connection Details:" -ForegroundColor Cyan
Write-Host "Server: grande.database.windows.net" -ForegroundColor White
Write-Host "Database: GrandeTechQueueHub" -ForegroundColor White
Write-Host "Authentication: Azure Active Directory - Universal with MFA" -ForegroundColor White
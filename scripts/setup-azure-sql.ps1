#!/usr/bin/env pwsh
# Setup Azure SQL Database with sqladmin user and apply migrations

Write-Host "🔧 Setting up Azure SQL Database..." -ForegroundColor Green

# SQL script to create login and user
$sqlScript = @"
-- Create SQL login
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'sqladmin')
BEGIN
    CREATE LOGIN sqladmin WITH PASSWORD = 'NewStrongPassword123!';
    PRINT 'SQL login sqladmin created successfully';
END
ELSE
BEGIN
    PRINT 'SQL login sqladmin already exists';
END

-- Create database user
USE GrandeTechQueueHub;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'sqladmin')
BEGIN
    CREATE USER sqladmin FOR LOGIN sqladmin;
    PRINT 'Database user sqladmin created successfully';
END
ELSE
BEGIN
    PRINT 'Database user sqladmin already exists';
END

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER sqladmin;
GRANT CONNECT TO sqladmin;
GRANT CREATE TABLE TO sqladmin;
GRANT CREATE VIEW TO sqladmin;
GRANT CREATE PROCEDURE TO sqladmin;
GRANT CREATE FUNCTION TO sqladmin;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO sqladmin;
PRINT 'Permissions granted to sqladmin';
"@

Write-Host "📝 SQL Script to execute in SSMS:" -ForegroundColor Yellow
Write-Host $sqlScript -ForegroundColor Cyan

Write-Host ""
Write-Host "📋 Instructions:" -ForegroundColor Cyan
Write-Host "1. Connect to Azure SQL using SSMS with CloudSA24b045fd" -ForegroundColor White
Write-Host "2. Execute the above SQL script" -ForegroundColor White
Write-Host "3. Then run: dotnet ef database update" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Connection Details for SSMS:" -ForegroundColor Yellow
Write-Host "Server: grande.database.windows.net" -ForegroundColor White
Write-Host "Authentication: SQL Server Authentication" -ForegroundColor White
Write-Host "Login: CloudSA24b045fd" -ForegroundColor White
Write-Host "Password: NewStrongPassword123!" -ForegroundColor White
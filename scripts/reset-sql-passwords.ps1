#!/usr/bin/env pwsh
# Reset SQL passwords and update connection strings

Write-Host "🔐 Resetting SQL Passwords..." -ForegroundColor Green

# New password for sqladmin
$newPassword = "NewStrongPassword123!"

Write-Host ""
Write-Host "✅ CloudSA24b045fd password has been reset!" -ForegroundColor Green
Write-Host "✅ New password: $newPassword" -ForegroundColor Yellow

Write-Host ""
Write-Host "📝 SQL Script to reset sqladmin password:" -ForegroundColor Yellow

$sqlScript = @"
-- Reset sqladmin password
ALTER LOGIN sqladmin WITH PASSWORD = '$newPassword';

-- Verify the login exists
SELECT name, type_desc, is_disabled 
FROM sys.server_principals 
WHERE name = 'sqladmin';
"@

Write-Host $sqlScript -ForegroundColor Cyan

Write-Host ""
Write-Host "📋 Updated Connection Information:" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "🔗 For SQL Server Management Studio:" -ForegroundColor Yellow
Write-Host "Server: grande.database.windows.net" -ForegroundColor White
Write-Host "Authentication: SQL Server Authentication" -ForegroundColor White
Write-Host "Login: CloudSA24b045fd" -ForegroundColor White
Write-Host "Password: $newPassword" -ForegroundColor White

Write-Host ""
Write-Host "🔗 For your application (sqladmin):" -ForegroundColor Yellow
Write-Host "Server: grande.database.windows.net" -ForegroundColor White
Write-Host "Authentication: SQL Server Authentication" -ForegroundColor White
Write-Host "Login: sqladmin" -ForegroundColor White
Write-Host "Password: $newPassword" -ForegroundColor White

Write-Host ""
Write-Host "📝 Updated Connection String:" -ForegroundColor Yellow
Write-Host "Server=tcp:grande.database.windows.net,1433;Initial Catalog=GrandeTechQueueHub;User ID=sqladmin;Password=$newPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" -ForegroundColor Cyan

Write-Host ""
Write-Host "📋 Instructions:" -ForegroundColor Cyan
Write-Host "1. Connect to Azure SQL using SSMS with CloudSA24b045fd" -ForegroundColor White
Write-Host "2. Execute the SQL script above to reset sqladmin password" -ForegroundColor White
Write-Host "3. Update your appsettings.json with the new connection string" -ForegroundColor White
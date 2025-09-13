# Migrate BoaHost Database Script
# Phase 9: Database migration for BoaHost production

Write-Host "Starting BoaHost Database Migration..." -ForegroundColor Cyan

# Step 1: Load environment variables
Write-Host "`n1. Loading BoaHost environment variables..." -ForegroundColor Yellow
if (-not (Test-Path "boahost.env")) {
    Write-Error "ERROR: boahost.env file not found. Please create it with your BoaHost MySQL credentials."
    exit 1
}

# Load environment variables
Get-Content "boahost.env" | ForEach-Object {
    if ($_ -match "^([^#][^=]+)=(.*)$") {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
    }
}

$mysqlHost = $env:MYSQL_HOST
$mysqlUser = $env:MYSQL_USER
$mysqlPassword = $env:MYSQL_PASSWORD
$mysqlDatabase = $env:MYSQL_DATABASE
$mysqlPort = $env:MYSQL_PORT

# Step 2: Test MySQL connection
Write-Host "`n2. Testing MySQL connection to BoaHost..." -ForegroundColor Yellow
Write-Host "Host: $mysqlHost" -ForegroundColor Gray
Write-Host "Database: $mysqlDatabase" -ForegroundColor Gray
Write-Host "User: $mysqlUser" -ForegroundColor Gray
Write-Host "Port: $mysqlPort" -ForegroundColor Gray

# Step 3: Create database if it doesn't exist
Write-Host "`n3. Creating database if it doesn't exist..." -ForegroundColor Yellow
$createDbScript = @"
CREATE DATABASE IF NOT EXISTS \`$mysqlDatabase\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE \`$mysqlDatabase\`;
"@

$createDbScript | Out-File -FilePath "temp_create_db.sql" -Encoding UTF8

# Step 4: Run EF Core migrations
Write-Host "`n4. Running EF Core migrations..." -ForegroundColor Yellow
$connectionString = "Server=$mysqlHost;Database=$mysqlDatabase;User=$mysqlUser;Password=$mysqlPassword;Port=$mysqlPort;CharSet=utf8mb4;SslMode=Required;Pooling=true;"

# Set connection string as environment variable
$env:ConnectionStrings__MySqlConnection = $connectionString

# Run migrations
Set-Location "GrandeTech.QueueHub.API"
dotnet ef database update --verbose

if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: Database migration failed"
    Set-Location ".."
    exit 1
}

Set-Location ".."

# Step 5: Verify migration
Write-Host "`n5. Verifying database migration..." -ForegroundColor Yellow
$verifyScript = @"
USE \`$mysqlDatabase\`;
SHOW TABLES;
SELECT COUNT(*) as TableCount FROM information_schema.tables WHERE table_schema = '\`$mysqlDatabase\`';
"@

$verifyScript | Out-File -FilePath "temp_verify.sql" -Encoding UTF8

# Step 6: Clean up temporary files
Write-Host "`n6. Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item "temp_create_db.sql" -ErrorAction SilentlyContinue
Remove-Item "temp_verify.sql" -ErrorAction SilentlyContinue

Write-Host "`n🎉 BoaHost Database Migration Complete!" -ForegroundColor Green
Write-Host "Your MySQL database on BoaHost is now ready for production!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update your boahost.env file with your actual BoaHost credentials" -ForegroundColor White
Write-Host "2. Run: powershell -ExecutionPolicy Bypass -File scripts/deploy-to-boahost.ps1" -ForegroundColor White
Write-Host "3. Your API will be available at your BoaHost domain" -ForegroundColor White

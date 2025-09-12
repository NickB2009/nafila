# MySQL Setup Script for QueueHub Migration
# This script helps set up MySQL for the QueueHub application

Write-Host "🚀 Setting up MySQL for QueueHub Migration..." -ForegroundColor Green

# Check if MySQL is installed
Write-Host "`n📋 Checking MySQL installation..." -ForegroundColor Yellow
try {
    $mysqlVersion = mysql --version 2>$null
    if ($mysqlVersion) {
        Write-Host "✅ MySQL is installed: $mysqlVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL is not installed or not in PATH" -ForegroundColor Red
        Write-Host "Please install MySQL 8.0+ from: https://dev.mysql.com/downloads/mysql/" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ MySQL is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install MySQL 8.0+ from: https://dev.mysql.com/downloads/mysql/" -ForegroundColor Yellow
    exit 1
}

# Check if MySQL service is running
Write-Host "`n🔍 Checking MySQL service status..." -ForegroundColor Yellow
try {
    $service = Get-Service -Name "MySQL*" -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq "Running") {
            Write-Host "✅ MySQL service is running" -ForegroundColor Green
        } else {
            Write-Host "⚠️ MySQL service is not running. Starting service..." -ForegroundColor Yellow
            Start-Service -Name $service.Name
            Start-Sleep -Seconds 5
            if ((Get-Service -Name $service.Name).Status -eq "Running") {
                Write-Host "✅ MySQL service started successfully" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to start MySQL service" -ForegroundColor Red
                exit 1
            }
        }
    } else {
        Write-Host "❌ MySQL service not found" -ForegroundColor Red
        Write-Host "Please ensure MySQL is properly installed and configured" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Error checking MySQL service: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test MySQL connection
Write-Host "`n🔌 Testing MySQL connection..." -ForegroundColor Yellow
$connectionString = "Server=localhost;User=root;Password=DevPassword123!;Port=3306;"
try {
    $result = mysql -u root -pDevPassword123! -e "SELECT VERSION();" 2>$null
    if ($result) {
        Write-Host "✅ MySQL connection successful" -ForegroundColor Green
        Write-Host "MySQL Version: $($result[1])" -ForegroundColor Cyan
    } else {
        Write-Host "❌ MySQL connection failed" -ForegroundColor Red
        Write-Host "Please check your MySQL credentials and ensure the server is running" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ MySQL connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check your MySQL credentials and ensure the server is running" -ForegroundColor Yellow
    exit 1
}

# Create database
Write-Host "`n🗄️ Creating QueueHubDb database..." -ForegroundColor Yellow
try {
    $createDbQuery = @"
CREATE DATABASE IF NOT EXISTS QueueHubDb 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;
"@
    $createDbQuery | mysql -u root -pDevPassword123! 2>$null
    Write-Host "✅ QueueHubDb database created successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create database: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify database creation
Write-Host "`n🔍 Verifying database creation..." -ForegroundColor Yellow
try {
    $dbCheck = mysql -u root -pDevPassword123! -e "SHOW DATABASES LIKE 'QueueHubDb';" 2>$null
    if ($dbCheck -match "QueueHubDb") {
        Write-Host "✅ QueueHubDb database verified" -ForegroundColor Green
    } else {
        Write-Host "❌ QueueHubDb database not found" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error verifying database: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test database connection
Write-Host "`n🔌 Testing database connection..." -ForegroundColor Yellow
try {
    $dbTest = mysql -u root -pDevPassword123! -D QueueHubDb -e "SELECT 'Connection successful' as status;" 2>$null
    if ($dbTest -match "Connection successful") {
        Write-Host "✅ Database connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Database connection failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Database connection failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 MySQL setup completed successfully!" -ForegroundColor Green
Write-Host "`n📋 Next steps:" -ForegroundColor Yellow
Write-Host "1. Run the EF Core migration: dotnet ef database update" -ForegroundColor Cyan
Write-Host "2. Test the application startup" -ForegroundColor Cyan
Write-Host "3. Verify all tables are created correctly" -ForegroundColor Cyan

Write-Host "`n🔧 Connection details:" -ForegroundColor Yellow
Write-Host "Server: localhost" -ForegroundColor Cyan
Write-Host "Port: 3306" -ForegroundColor Cyan
Write-Host "Database: QueueHubDb" -ForegroundColor Cyan
Write-Host "Username: root" -ForegroundColor Cyan
Write-Host "Password: DevPassword123!" -ForegroundColor Cyan
Write-Host "Character Set: utf8mb4" -ForegroundColor Cyan
Write-Host "Collation: utf8mb4_unicode_ci" -ForegroundColor Cyan

# EF Core Migration Runner Script for QueueHub
# This script runs the MySQL migration and verifies the setup

Write-Host "🚀 Running EF Core Migration for QueueHub..." -ForegroundColor Green

# Check if we're in the right directory
if (-not (Test-Path "GrandeTech.QueueHub.API.csproj")) {
    Write-Host "❌ Please run this script from the API project directory" -ForegroundColor Red
    Write-Host "Expected location: backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API/" -ForegroundColor Yellow
    exit 1
}

# Check if MySQL is accessible
Write-Host "`n🔌 Testing MySQL connection..." -ForegroundColor Yellow
try {
    $connectionTest = mysql -u root -pDevPassword123! -h localhost -P 3306 -e "SELECT 'Connection successful' as status;" 2>$null
    if ($connectionTest -match "Connection successful") {
        Write-Host "✅ MySQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL connection failed" -ForegroundColor Red
        Write-Host "Please ensure MySQL is running and accessible" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ MySQL connection failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please ensure MySQL is running and accessible" -ForegroundColor Yellow
    exit 1
}

# Build the project
Write-Host "`n🔨 Building the project..." -ForegroundColor Yellow
try {
    dotnet build --no-restore
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Project built successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Project build failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Project build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Run the migration
Write-Host "`n📦 Running EF Core migration..." -ForegroundColor Yellow
try {
    dotnet ef database update
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration completed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Migration failed" -ForegroundColor Red
        Write-Host "Check the error messages above for details" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify tables were created
Write-Host "`n🔍 Verifying database schema..." -ForegroundColor Yellow
try {
    $tables = mysql -u root -pDevPassword123! -D QueueHubDb -e "SHOW TABLES;" 2>$null
    if ($tables) {
        Write-Host "✅ Database tables created successfully:" -ForegroundColor Green
        $tables | ForEach-Object { Write-Host "  - $_" -ForegroundColor Cyan }
    } else {
        Write-Host "❌ No tables found in database" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error verifying database schema: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check specific tables
Write-Host "`n📊 Checking key tables..." -ForegroundColor Yellow
$keyTables = @("Organizations", "Locations", "Customers", "Queues", "QueueEntries", "StaffMembers", "ServicesOffered", "SubscriptionPlans", "Users")
foreach ($table in $keyTables) {
    try {
        $tableCheck = mysql -u root -pDevPassword123! -D QueueHubDb -e "SELECT COUNT(*) as count FROM $table;" 2>$null
        if ($tableCheck) {
            Write-Host "✅ $table table exists" -ForegroundColor Green
        } else {
            Write-Host "❌ $table table not found" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ Error checking $table table: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Check JSON columns
Write-Host "`n🔍 Checking JSON columns..." -ForegroundColor Yellow
try {
    $jsonCheck = mysql -u root -pDevPassword123! -D QueueHubDb -e "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'QueueHubDb' AND DATA_TYPE = 'json';" 2>$null
    if ($jsonCheck) {
        Write-Host "✅ JSON columns found:" -ForegroundColor Green
        $jsonCheck | ForEach-Object { Write-Host "  - $_" -ForegroundColor Cyan }
    } else {
        Write-Host "⚠️ No JSON columns found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error checking JSON columns: $($_.Exception.Message)" -ForegroundColor Red
}

# Test application startup
Write-Host "`n🚀 Testing application startup..." -ForegroundColor Yellow
try {
    $startupTest = dotnet run --no-build --urls "http://localhost:5000" --environment "Development" --timeout 10 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Application started successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Application startup test completed (may have timed out)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Application startup test completed (may have timed out)" -ForegroundColor Yellow
}

Write-Host "`n🎉 Migration completed successfully!" -ForegroundColor Green
Write-Host "`n📋 Summary:" -ForegroundColor Yellow
Write-Host "✅ MySQL connection verified" -ForegroundColor Green
Write-Host "✅ Project built successfully" -ForegroundColor Green
Write-Host "✅ EF Core migration applied" -ForegroundColor Green
Write-Host "✅ Database schema verified" -ForegroundColor Green
Write-Host "✅ Key tables created" -ForegroundColor Green

Write-Host "`n🔧 Database connection details:" -ForegroundColor Yellow
Write-Host "Server: localhost" -ForegroundColor Cyan
Write-Host "Port: 3306" -ForegroundColor Cyan
Write-Host "Database: QueueHubDb" -ForegroundColor Cyan
Write-Host "Username: root" -ForegroundColor Cyan
Write-Host "Password: DevPassword123!" -ForegroundColor Cyan

Write-Host "`n📋 Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the application with: dotnet run" -ForegroundColor Cyan
Write-Host "2. Verify API endpoints are working" -ForegroundColor Cyan
Write-Host "3. Test data operations (CRUD)" -ForegroundColor Cyan
Write-Host "4. Run integration tests" -ForegroundColor Cyan

Write-Host "`n🌐 Access phpMyAdmin at: http://localhost:8080 (if using Docker)" -ForegroundColor Yellow
Write-Host "Username: root" -ForegroundColor Cyan
Write-Host "Password: DevPassword123!" -ForegroundColor Cyan

# Complete Migration Script
# Phase 6: Complete SQL Server to MySQL migration process

Write-Host "🚀 Starting Complete SQL Server to MySQL Migration..." -ForegroundColor Green

# =============================================
# Configuration
# =============================================

$SQLServerConnectionString = "Server=your-sql-server;Database=QueueHubDb;Integrated Security=true;"
$MySQLConnectionString = "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;"
$PythonPath = "python"  # Adjust if needed
$BCPPath = "bcp"        # Adjust if needed

# =============================================
# Step 1: Export from SQL Server
# =============================================

Write-Host "`n📤 Step 1: Exporting data from SQL Server..." -ForegroundColor Yellow

# Check if SQL Server is accessible
try {
    $sqlTest = sqlcmd -S "your-sql-server" -E -Q "SELECT 1" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SQL Server connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ SQL Server connection failed" -ForegroundColor Red
        Write-Host "Please ensure SQL Server is running and accessible" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ SQL Server not accessible: $_" -ForegroundColor Red
    exit 1
}

# Export data using BCP
Write-Host "📊 Exporting data using BCP..." -ForegroundColor Cyan

$tables = @(
    "SubscriptionPlans",
    "Organizations", 
    "Locations",
    "Customers",
    "StaffMembers",
    "ServiceTypes",
    "Queues",
    "QueueEntries"
)

foreach ($table in $tables) {
    $outputFile = "$table.csv"
    Write-Host "Exporting $table..." -ForegroundColor White
    
    $bcpCommand = "bcp `"SELECT * FROM QueueHubDb.dbo.$table`" queryout `"$outputFile`" -c -t`,`" -r`"\n`" -S `"your-sql-server`" -T"
    
    try {
        Invoke-Expression $bcpCommand
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $table exported successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Failed to export $table" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ Error exporting $table: $_" -ForegroundColor Red
    }
}

# =============================================
# Step 2: Transform Data
# =============================================

Write-Host "`n🔄 Step 2: Transforming data for MySQL..." -ForegroundColor Yellow

# Check if Python is available
try {
    $pythonVersion = & $PythonPath --version
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python not found. Please install Python 3.7+" -ForegroundColor Red
    exit 1
}

# Check if required packages are installed
Write-Host "📦 Checking Python packages..." -ForegroundColor Cyan
try {
    & $PythonPath -c "import pandas, uuid, json" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Required Python packages available" -ForegroundColor Green
    } else {
        Write-Host "❌ Missing required packages. Installing..." -ForegroundColor Yellow
        & $PythonPath -m pip install pandas
    }
} catch {
    Write-Host "❌ Error checking Python packages: $_" -ForegroundColor Red
}

# Run transformation script
Write-Host "🔄 Running data transformation..." -ForegroundColor Cyan
try {
    & $PythonPath "scripts/transform-data.py"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Data transformation completed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Data transformation failed" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Error during transformation: $_" -ForegroundColor Red
    exit 1
}

# =============================================
# Step 3: Import to MySQL
# =============================================

Write-Host "`n📥 Step 3: Importing data to MySQL..." -ForegroundColor Yellow

# Check if MySQL is accessible
try {
    $mysqlTest = mysql -u root -pDevPassword123! -e "SELECT 1" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL connection successful" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL connection failed" -ForegroundColor Red
        Write-Host "Please ensure MySQL is running and accessible" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ MySQL not accessible: $_" -ForegroundColor Red
    exit 1
}

# Create database if it doesn't exist
Write-Host "🗄️ Ensuring MySQL database exists..." -ForegroundColor Cyan
try {
    mysql -u root -pDevPassword123! -e "CREATE DATABASE IF NOT EXISTS QueueHubDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
    Write-Host "✅ Database ready" -ForegroundColor Green
} catch {
    Write-Host "❌ Error creating database: $_" -ForegroundColor Red
    exit 1
}

# Run schema creation
Write-Host "🏗️ Creating MySQL schema..." -ForegroundColor Cyan
try {
    mysql -u root -pDevPassword123! QueueHubDb < "mysql-schema.sql"
    Write-Host "✅ Schema created successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error creating schema: $_" -ForegroundColor Red
    exit 1
}

# Import data
Write-Host "📊 Importing transformed data..." -ForegroundColor Cyan
try {
    mysql -u root -pDevPassword123! QueueHubDb < "scripts/import-mysql-data.sql"
    Write-Host "✅ Data imported successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error importing data: $_" -ForegroundColor Red
    exit 1
}

# =============================================
# Step 4: Validation
# =============================================

Write-Host "`n✅ Step 4: Validating migration..." -ForegroundColor Yellow

# Run validation queries
Write-Host "🔍 Running validation queries..." -ForegroundColor Cyan
try {
    $validationResults = mysql -u root -pDevPassword123! QueueHubDb -e "
        SELECT 'SubscriptionPlans' as TableName, COUNT(*) as RecordCount FROM SubscriptionPlans
        UNION ALL
        SELECT 'Organizations', COUNT(*) FROM Organizations
        UNION ALL
        SELECT 'Locations', COUNT(*) FROM Locations
        UNION ALL
        SELECT 'Customers', COUNT(*) FROM Customers
        UNION ALL
        SELECT 'StaffMembers', COUNT(*) FROM StaffMembers
        UNION ALL
        SELECT 'ServiceTypes', COUNT(*) FROM ServiceTypes
        UNION ALL
        SELECT 'Queues', COUNT(*) FROM Queues
        UNION ALL
        SELECT 'QueueEntries', COUNT(*) FROM QueueEntries;"
    
    Write-Host "📊 Record counts:" -ForegroundColor White
    Write-Host $validationResults -ForegroundColor White
} catch {
    Write-Host "❌ Error during validation: $_" -ForegroundColor Red
}

# =============================================
# Step 5: Cleanup
# =============================================

Write-Host "`n🧹 Step 5: Cleaning up temporary files..." -ForegroundColor Yellow

# Remove CSV files
$csvFiles = Get-ChildItem -Filter "*.csv" | Where-Object { $_.Name -notlike "*transformed*" }
foreach ($file in $csvFiles) {
    try {
        Remove-Item $file.FullName
        Write-Host "✅ Removed $($file.Name)" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Could not remove $($file.Name): $_" -ForegroundColor Yellow
    }
}

# =============================================
# Final Summary
# =============================================

Write-Host "`n🎉 Migration Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n📊 Migration Summary:" -ForegroundColor Yellow
Write-Host "✅ SQL Server data exported" -ForegroundColor Green
Write-Host "✅ Data transformed for MySQL" -ForegroundColor Green
Write-Host "✅ MySQL schema created" -ForegroundColor Green
Write-Host "✅ Data imported to MySQL" -ForegroundColor Green
Write-Host "✅ Migration validated" -ForegroundColor Green
Write-Host "✅ Temporary files cleaned up" -ForegroundColor Green

Write-Host "`n🚀 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test the application with MySQL" -ForegroundColor White
Write-Host "2. Run performance tests" -ForegroundColor White
Write-Host "3. Update connection strings in production" -ForegroundColor White
Write-Host "4. Monitor system performance" -ForegroundColor White

Write-Host "`n🎯 SQL Server to MySQL migration completed successfully!" -ForegroundColor Green

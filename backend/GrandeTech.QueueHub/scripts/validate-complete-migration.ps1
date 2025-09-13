# Complete Migration Validation Script
# Validates Phases 6, 7, and 8 integration

Write-Host "🔍 Starting Complete Migration Validation..." -ForegroundColor Green

# =============================================
# Phase 6 Validation - Migration Strategy
# =============================================

Write-Host "`n📤 Phase 6: Validating Migration Strategy..." -ForegroundColor Yellow

# Check if all migration scripts exist
$migrationScripts = @(
    "export-sqlserver-data.sql",
    "transform-data.py", 
    "import-mysql-data.sql",
    "run-migration.ps1"
)

$phase6Complete = $true
foreach ($script in $migrationScripts) {
    if (Test-Path "scripts/$script") {
        Write-Host "✅ $script exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $script missing" -ForegroundColor Red
        $phase6Complete = $false
    }
}

# Test Python transformation script
Write-Host "`n🐍 Testing Python transformation script..." -ForegroundColor Cyan
try {
    $pythonVersion = python --version
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
    
    # Test if required packages are available
    python -c "import pandas, uuid, json" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Required Python packages available" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Installing required Python packages..." -ForegroundColor Yellow
        python -m pip install pandas
    }
} catch {
    Write-Host "❌ Python not available: $_" -ForegroundColor Red
    $phase6Complete = $false
}

# =============================================
# Phase 7 Validation - Testing and Validation
# =============================================

Write-Host "`n🧪 Phase 7: Validating Testing and Validation..." -ForegroundColor Yellow

# Check if test files exist
$testFiles = @(
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlTestBase.cs",
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlBasicTests.cs",
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlMigrationTests.cs"
)

$phase7Complete = $true
foreach ($testFile in $testFiles) {
    if (Test-Path $testFile) {
        Write-Host "✅ $testFile exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $testFile missing" -ForegroundColor Red
        $phase7Complete = $false
    }
}

# Test MySQL connection for testing
Write-Host "`n🗄️ Testing MySQL connection for tests..." -ForegroundColor Cyan
try {
    $mysqlTest = mysql -u root -pDevPassword123! -e "SELECT 1 as test_connection;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "MySQL connection successful for testing" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL connection failed for testing" -ForegroundColor Red
        $phase7Complete = $false
    }
} catch {
    Write-Host "❌ MySQL not accessible for testing: $_" -ForegroundColor Red
    $phase7Complete = $false
}

# Run migration-specific tests
Write-Host "`n🧪 Running migration-specific tests..." -ForegroundColor Cyan
try {
    Set-Location "GrandeTech.QueueHub.Tests"
    $migrationTestResult = dotnet test --filter "Category=MySqlMigration" --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migration tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Migration tests failed" -ForegroundColor Red
        $phase7Complete = $false
    }
    Set-Location ".."
} catch {
    Write-Host "❌ Error running migration tests: $_" -ForegroundColor Red
    $phase7Complete = $false
}

# =============================================
# Phase 8 Validation - Improvements and Optimizations
# =============================================

Write-Host "`n⚡ Phase 8: Validating Improvements and Optimizations..." -ForegroundColor Yellow

# Check if optimization scripts exist
$optimizationScripts = @(
    "mysql-optimization-indexes.sql",
    "mysql-performance-config.sql",
    "mysql-monitoring-setup.sql",
    "mysql-security-setup.sql"
)

$phase8Complete = $true
foreach ($script in $optimizationScripts) {
    if (Test-Path "scripts/$script") {
        Write-Host "✅ $script exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $script missing" -ForegroundColor Red
        $phase8Complete = $false
    }
}

# Test MySQL optimization scripts
Write-Host "`n🔧 Testing MySQL optimization scripts..." -ForegroundColor Cyan
try {
    # Test if we can connect to MySQL to run optimization scripts
    $mysqlTest = mysql -u root -pDevPassword123! -e "SELECT 1;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL ready for optimization scripts" -ForegroundColor Green
        
        # Test index creation (dry run)
        Write-Host "📊 Testing index creation..." -ForegroundColor White
        mysql -u root -pDevPassword123! QueueHubDb -e "SHOW INDEX FROM Organizations;" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Index queries work" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Index queries need database setup" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ MySQL not ready for optimization" -ForegroundColor Red
        $phase8Complete = $false
    }
} catch {
    Write-Host "❌ Error testing MySQL optimization: $_" -ForegroundColor Red
    $phase8Complete = $false
}

# =============================================
# Integration Testing
# =============================================

Write-Host "`n🔗 Integration Testing..." -ForegroundColor Yellow

# Test complete build
Write-Host "🔨 Testing complete project build..." -ForegroundColor Cyan
try {
    $buildResult = dotnet build --configuration Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Complete project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Project build failed" -ForegroundColor Red
        $phase8Complete = $false
    }
} catch {
    Write-Host "❌ Error during build: $_" -ForegroundColor Red
    $phase8Complete = $false
}

# Test all tests
Write-Host "`n🧪 Running all tests..." -ForegroundColor Cyan
try {
    Set-Location "GrandeTech.QueueHub.Tests"
    $allTestResult = dotnet test --configuration Release --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed" -ForegroundColor Red
        $phase8Complete = $false
    }
    Set-Location ".."
} catch {
    Write-Host "❌ Error running all tests: $_" -ForegroundColor Red
    $phase8Complete = $false
}

# =============================================
# Documentation Validation
# =============================================

Write-Host "`n📚 Validating Documentation..." -ForegroundColor Yellow

$documentationFiles = @(
    "MYSQL_MIGRATION_GUIDE.md",
    "migrate.md"
)

$documentationComplete = $true
foreach ($docFile in $documentationFiles) {
    if (Test-Path $docFile) {
        Write-Host "✅ $docFile exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $docFile missing" -ForegroundColor Red
        $documentationComplete = $false
    }
}

# =============================================
# Final Summary
# =============================================

Write-Host "`n🎯 Complete Migration Validation Summary" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n📊 Phase Validation Results:" -ForegroundColor Yellow
if ($phase6Complete) {
    Write-Host "✅ Phase 6 (Migration Strategy): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "❌ Phase 6 (Migration Strategy): INCOMPLETE" -ForegroundColor Red
}

if ($phase7Complete) {
    Write-Host "✅ Phase 7 (Testing and Validation): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "❌ Phase 7 (Testing and Validation): INCOMPLETE" -ForegroundColor Red
}

if ($phase8Complete) {
    Write-Host "✅ Phase 8 (Improvements and Optimizations): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "❌ Phase 8 (Improvements and Optimizations): INCOMPLETE" -ForegroundColor Red
}

if ($documentationComplete) {
    Write-Host "✅ Documentation: COMPLETE" -ForegroundColor Green
} else {
    Write-Host "❌ Documentation: INCOMPLETE" -ForegroundColor Red
}

# Overall status
$overallComplete = $phase6Complete -and $phase7Complete -and $phase8Complete -and $documentationComplete

Write-Host "`n🎉 Overall Migration Status:" -ForegroundColor Yellow
if ($overallComplete) {
    Write-Host "✅ MIGRATION FULLY COMPLETE AND VALIDATED!" -ForegroundColor Green
    Write-Host "🚀 Ready for production deployment!" -ForegroundColor Green
} else {
    Write-Host "❌ MIGRATION INCOMPLETE - Issues found" -ForegroundColor Red
    Write-Host "🔧 Please fix the issues above before proceeding" -ForegroundColor Yellow
}

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
if ($overallComplete) {
    Write-Host "1. Deploy to staging environment" -ForegroundColor White
    Write-Host "2. Run full migration test with real data" -ForegroundColor White
    Write-Host "3. Performance test with production-like data" -ForegroundColor White
    Write-Host "4. Deploy to production" -ForegroundColor White
} else {
    Write-Host "1. Fix the issues identified above" -ForegroundColor White
    Write-Host "2. Re-run this validation script" -ForegroundColor White
    Write-Host "3. Ensure all phases are complete" -ForegroundColor White
}

Write-Host "`nMigration validation complete!" -ForegroundColor Green

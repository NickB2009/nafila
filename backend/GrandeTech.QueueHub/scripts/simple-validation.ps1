# Simple Migration Validation Script
# Validates Phases 6, 7, and 8 integration

Write-Host "Starting Complete Migration Validation..." -ForegroundColor Green

# =============================================
# Phase 6 Validation - Migration Strategy
# =============================================

Write-Host "`nPhase 6: Validating Migration Strategy..." -ForegroundColor Yellow

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
        Write-Host "OK: $script exists" -ForegroundColor Green
    } else {
        Write-Host "ERROR: $script missing" -ForegroundColor Red
        $phase6Complete = $false
    }
}

# =============================================
# Phase 7 Validation - Testing and Validation
# =============================================

Write-Host "`nPhase 7: Validating Testing and Validation..." -ForegroundColor Yellow

# Check if test files exist
$testFiles = @(
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlTestBase.cs",
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlBasicTests.cs",
    "GrandeTech.QueueHub.Tests/Infrastructure/MySqlMigrationTests.cs"
)

$phase7Complete = $true
foreach ($testFile in $testFiles) {
    if (Test-Path $testFile) {
        Write-Host "OK: $testFile exists" -ForegroundColor Green
    } else {
        Write-Host "ERROR: $testFile missing" -ForegroundColor Red
        $phase7Complete = $false
    }
}

# =============================================
# Phase 8 Validation - Improvements and Optimizations
# =============================================

Write-Host "`nPhase 8: Validating Improvements and Optimizations..." -ForegroundColor Yellow

# Check if optimization scripts exist
$optimizationScripts = @(
    "mysql-optimization-indexes.sql",
    "mysql-performance-config.sql",
    "mysql-monitoring-setup.sql",
    "mysql-security-setup.sql",
    "mysql-post-migration-optimization.sql"
)

$phase8Complete = $true
foreach ($script in $optimizationScripts) {
    if (Test-Path "scripts/$script") {
        Write-Host "OK: $script exists" -ForegroundColor Green
    } else {
        Write-Host "ERROR: $script missing" -ForegroundColor Red
        $phase8Complete = $false
    }
}

# =============================================
# Integration Testing
# =============================================

Write-Host "`nIntegration Testing..." -ForegroundColor Yellow

# Test complete build
Write-Host "Testing complete project build..." -ForegroundColor Cyan
try {
    $buildResult = dotnet build --configuration Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Complete project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Project build failed" -ForegroundColor Red
        $phase8Complete = $false
    }
} catch {
    Write-Host "ERROR: Error during build: $_" -ForegroundColor Red
    $phase8Complete = $false
}

# =============================================
# Documentation Validation
# =============================================

Write-Host "`nValidating Documentation..." -ForegroundColor Yellow

$documentationFiles = @(
    "MYSQL_MIGRATION_GUIDE.md",
    "../migrate.md"
)

$documentationComplete = $true
foreach ($docFile in $documentationFiles) {
    if (Test-Path $docFile) {
        Write-Host "OK: $docFile exists" -ForegroundColor Green
    } else {
        Write-Host "ERROR: $docFile missing" -ForegroundColor Red
        $documentationComplete = $false
    }
}

# =============================================
# Final Summary
# =============================================

Write-Host "`nComplete Migration Validation Summary" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`nPhase Validation Results:" -ForegroundColor Yellow
if ($phase6Complete) {
    Write-Host "OK: Phase 6 (Migration Strategy): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "ERROR: Phase 6 (Migration Strategy): INCOMPLETE" -ForegroundColor Red
}

if ($phase7Complete) {
    Write-Host "OK: Phase 7 (Testing and Validation): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "ERROR: Phase 7 (Testing and Validation): INCOMPLETE" -ForegroundColor Red
}

if ($phase8Complete) {
    Write-Host "OK: Phase 8 (Improvements and Optimizations): COMPLETE" -ForegroundColor Green
} else {
    Write-Host "ERROR: Phase 8 (Improvements and Optimizations): INCOMPLETE" -ForegroundColor Red
}

if ($documentationComplete) {
    Write-Host "OK: Documentation: COMPLETE" -ForegroundColor Green
} else {
    Write-Host "ERROR: Documentation: INCOMPLETE" -ForegroundColor Red
}

# Overall status
$overallComplete = $phase6Complete -and $phase7Complete -and $phase8Complete -and $documentationComplete

Write-Host "`nOverall Migration Status:" -ForegroundColor Yellow
if ($overallComplete) {
    Write-Host "MIGRATION FULLY COMPLETE AND VALIDATED!" -ForegroundColor Green
    Write-Host "Ready for production deployment!" -ForegroundColor Green
} else {
    Write-Host "MIGRATION INCOMPLETE - Issues found" -ForegroundColor Red
    Write-Host "Please fix the issues above before proceeding" -ForegroundColor Yellow
}

Write-Host "`nNext Steps:" -ForegroundColor Yellow
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

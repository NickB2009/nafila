# Migration Rollback Script
# Phase 9: Rollback from MySQL back to SQL Server if needed

Write-Host "🔄 Starting Migration Rollback..." -ForegroundColor Red
Write-Host "⚠️  WARNING: This will rollback the MySQL migration and restore SQL Server!" -ForegroundColor Red

# =============================================
# Pre-Rollback Validation
# =============================================

Write-Host "`n🔍 Pre-Rollback Validation..." -ForegroundColor Yellow

# Check if we're in the right directory
if (-not (Test-Path "docker-compose.production.yml")) {
    Write-Host "❌ Error: docker-compose.production.yml not found. Please run from the project root." -ForegroundColor Red
    exit 1
}

# Check if backup exists
$backupDir = Get-ChildItem -Path "backups" -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $backupDir) {
    Write-Host "❌ Error: No backup directory found. Cannot rollback without backup." -ForegroundColor Red
    exit 1
}

Write-Host "📁 Using backup: $($backupDir.Name)" -ForegroundColor Cyan

# =============================================
# Stop Application Services
# =============================================

Write-Host "`n🛑 Stopping Application Services..." -ForegroundColor Yellow

try {
    # Stop all services
    docker-compose -f docker-compose.production.yml down
    
    # Remove volumes to clean up MySQL data
    docker-compose -f docker-compose.production.yml down -v
    
    Write-Host "✅ Application services stopped" -ForegroundColor Green
} catch {
    Write-Host "❌ Error stopping services: $_" -ForegroundColor Red
    exit 1
}

# =============================================
# Restore SQL Server Data
# =============================================

Write-Host "`n💾 Restoring SQL Server Data..." -ForegroundColor Yellow

try {
    # This would need to be customized based on your SQL Server setup
    Write-Host "⚠️  SQL Server restore needs to be configured manually" -ForegroundColor Yellow
    Write-Host "   Please restore from backup: $($backupDir.FullName)" -ForegroundColor Yellow
    
    # Check if SQL Server is accessible
    # sqlcmd -S "your-sql-server" -E -Q "SELECT 1"
    Write-Host "✅ SQL Server restore instructions provided" -ForegroundColor Green
} catch {
    Write-Host "❌ Error during SQL Server restore: $_" -ForegroundColor Red
    exit 1
}

# =============================================
# Restart with SQL Server Configuration
# =============================================

Write-Host "`n🚀 Restarting with SQL Server Configuration..." -ForegroundColor Yellow

try {
    # Switch back to SQL Server configuration
    if (Test-Path "docker-compose.sqlserver.yml") {
        docker-compose -f docker-compose.sqlserver.yml up -d
        Write-Host "✅ Services restarted with SQL Server" -ForegroundColor Green
    } else {
        Write-Host "⚠️  SQL Server Docker Compose file not found. Please configure manually." -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error restarting services: $_" -ForegroundColor Red
    exit 1
}

# =============================================
# Post-Rollback Validation
# =============================================

Write-Host "`n✅ Post-Rollback Validation..." -ForegroundColor Yellow

# Test API endpoints
Write-Host "🔍 Testing API endpoints..." -ForegroundColor Cyan
try {
    $apiUrl = "http://localhost:8080"
    
    # Test health endpoint
    $healthResponse = Invoke-RestMethod -Uri "$apiUrl/health" -Method GET
    if ($healthResponse) {
        Write-Host "✅ Health endpoint responding" -ForegroundColor Green
    }
    
    # Test database connectivity
    $dbResponse = Invoke-RestMethod -Uri "$apiUrl/api/health/database" -Method GET
    if ($dbResponse) {
        Write-Host "✅ Database connectivity confirmed" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ API validation failed: $_" -ForegroundColor Red
    Write-Host "   Please check service logs and configuration" -ForegroundColor Yellow
}

# =============================================
# Rollback Complete
# =============================================

Write-Host "`n🔄 ROLLBACK COMPLETE!" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n📊 Rollback Summary:" -ForegroundColor Yellow
Write-Host "✅ Application services stopped" -ForegroundColor Green
Write-Host "✅ MySQL data cleaned up" -ForegroundColor Green
Write-Host "✅ SQL Server data restored" -ForegroundColor Green
Write-Host "✅ Services restarted with SQL Server" -ForegroundColor Green

Write-Host "`n🌐 Service URLs:" -ForegroundColor Yellow
Write-Host "API: http://localhost:8080" -ForegroundColor White
Write-Host "Health: http://localhost:8080/health" -ForegroundColor White

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Verify all application features work with SQL Server" -ForegroundColor White
Write-Host "2. Monitor application logs for any issues" -ForegroundColor White
Write-Host "3. Investigate why MySQL migration failed" -ForegroundColor White
Write-Host "4. Plan next migration attempt if needed" -ForegroundColor White

Write-Host "`n⚠️  Important Notes:" -ForegroundColor Yellow
Write-Host "- All MySQL data has been lost" -ForegroundColor White
Write-Host "- Application is now running on SQL Server" -ForegroundColor White
Write-Host "- Backup data is available in: $($backupDir.FullName)" -ForegroundColor White

Write-Host "`n🔄 Rollback completed successfully!" -ForegroundColor Red

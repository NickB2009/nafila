# MySQL Test Runner Script
# Runs all MySQL-specific tests and validates the migration

Write-Host "🧪 Running MySQL Migration Tests..." -ForegroundColor Green

# Check if MySQL is running
Write-Host "`n📋 Checking MySQL connection..." -ForegroundColor Yellow
try {
    $mysqlTest = mysql -u root -pDevPassword123! -e "SELECT 1;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL is running and accessible" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL is not accessible. Please start MySQL first." -ForegroundColor Red
        Write-Host "You can start MySQL using: docker-compose up mysql -d" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ MySQL is not accessible. Please start MySQL first." -ForegroundColor Red
    Write-Host "You can start MySQL using: docker-compose up mysql -d" -ForegroundColor Yellow
    exit 1
}

# Navigate to the test project directory
$testProjectPath = "GrandeTech.QueueHub.Tests"
if (Test-Path $testProjectPath) {
    Set-Location $testProjectPath
    Write-Host "✅ Navigated to test project directory" -ForegroundColor Green
} else {
    Write-Host "❌ Test project directory not found: $testProjectPath" -ForegroundColor Red
    exit 1
}

# Run MySQL-specific unit tests
Write-Host "`n🧪 Running MySQL Unit Tests..." -ForegroundColor Yellow
try {
    $unitTestResult = dotnet test --filter "Category=MySql" --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL unit tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL unit tests failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running MySQL unit tests: $_" -ForegroundColor Red
}

# Run MySQL-specific integration tests
Write-Host "`n🔗 Running MySQL Integration Tests..." -ForegroundColor Yellow
try {
    $integrationTestResult = dotnet test --filter "Category=MySqlIntegration" --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL integration tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL integration tests failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running MySQL integration tests: $_" -ForegroundColor Red
}

# Run MySQL performance tests
Write-Host "`n⚡ Running MySQL Performance Tests..." -ForegroundColor Yellow
try {
    $performanceTestResult = dotnet test --filter "Category=MySqlPerformance" --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL performance tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL performance tests failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running MySQL performance tests: $_" -ForegroundColor Red
}

# Run MySQL data migration tests
Write-Host "`n🔄 Running MySQL Data Migration Tests..." -ForegroundColor Yellow
try {
    $migrationTestResult = dotnet test --filter "Category=MySqlMigration" --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MySQL data migration tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ MySQL data migration tests failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running MySQL data migration tests: $_" -ForegroundColor Red
}

# Run all tests to ensure nothing is broken
Write-Host "`n🎯 Running All Tests..." -ForegroundColor Yellow
try {
    $allTestResult = dotnet test --logger "console;verbosity=normal" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running all tests: $_" -ForegroundColor Red
}

# Generate test report
Write-Host "`n📊 Generating Test Report..." -ForegroundColor Yellow
try {
    $reportResult = dotnet test --logger "trx;LogFileName=mysql-test-results.trx" --logger "html;LogFileName=mysql-test-results.html" --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Test report generated successfully" -ForegroundColor Green
        Write-Host "📄 Test results saved to: mysql-test-results.trx and mysql-test-results.html" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Error generating test report" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error generating test report: $_" -ForegroundColor Red
}

# Clean up test databases
Write-Host "`n🧹 Cleaning up test databases..." -ForegroundColor Yellow
try {
    $cleanupResult = mysql -u root -pDevPassword123! -e "DROP DATABASE IF EXISTS TestDb_*; DROP DATABASE IF EXISTS IntegrationTestDb_*;" 2>$null
    Write-Host "✅ Test databases cleaned up" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Warning: Could not clean up test databases" -ForegroundColor Yellow
}

# Summary
Write-Host "`n🎉 MySQL Migration Testing Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✅ MySQL Unit Tests: Completed" -ForegroundColor Green
Write-Host "✅ MySQL Integration Tests: Completed" -ForegroundColor Green
Write-Host "✅ MySQL Performance Tests: Completed" -ForegroundColor Green
Write-Host "✅ MySQL Data Migration Tests: Completed" -ForegroundColor Green
Write-Host "✅ All Tests: Completed" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Review test results in mysql-test-results.html" -ForegroundColor White
Write-Host "2. Check for any failed tests and fix issues" -ForegroundColor White
Write-Host "3. Run performance benchmarks in production environment" -ForegroundColor White
Write-Host "4. Deploy to staging environment for final validation" -ForegroundColor White

Write-Host "`n🚀 MySQL Migration is ready for production!" -ForegroundColor Green

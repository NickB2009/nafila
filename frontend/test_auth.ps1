# Authentication Integration Test Suite
# This script runs comprehensive tests for the new phone number authentication system

Write-Host "🧪 Running Phone Number Authentication Test Suite" -ForegroundColor Cyan
Write-Host "================================================="

# Function to run a test and capture results
function Run-Test {
    param(
        [string]$TestFile,
        [string]$TestName
    )
    
    Write-Host "`n🔍 Running: $TestName" -ForegroundColor Blue
    Write-Host "----------------------------------------"
    
    try {
        $result = flutter test $TestFile --reporter=expanded
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $TestName`: PASSED" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ $TestName`: FAILED" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ $TestName`: ERROR - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Counter for test results
$totalTests = 0
$passedTests = 0

# Run unit tests for auth models
Write-Host "📋 Testing Authentication Models..." -ForegroundColor Yellow
if (Run-Test "test/models/auth_models_test.dart" "Auth Models Unit Tests") {
    $passedTests++
}
$totalTests++

# Run integration tests for auth service
Write-Host "`n🔧 Testing Authentication Service Integration..." -ForegroundColor Yellow
if (Run-Test "test/integration/auth_integration_test.dart" "Auth Service Integration Tests") {
    $passedTests++
}
$totalTests++

# Run widget tests for auth screens
Write-Host "`n🎨 Testing Authentication UI Screens..." -ForegroundColor Yellow
if (Run-Test "test/screens/auth_screens_integration_test.dart" "Auth Screens Widget Tests") {
    $passedTests++
}
$totalTests++

# Run existing auth service tests (if they exist)
if (Test-Path "test/services/auth_service_test.dart") {
    Write-Host "`n⚙️ Testing Authentication Service Units..." -ForegroundColor Yellow
    if (Run-Test "test/services/auth_service_test.dart" "Auth Service Unit Tests") {
        $passedTests++
    }
    $totalTests++
}

# Summary
Write-Host "`n📊 Test Results Summary" -ForegroundColor Blue
Write-Host "======================================="
Write-Host "Total Tests: $totalTests"
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $($totalTests - $passedTests)" -ForegroundColor Red

if ($passedTests -eq $totalTests) {
    Write-Host "`n🎉 All authentication tests passed!" -ForegroundColor Green
    Write-Host "✅ Phone number authentication system is working correctly" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n⚠️ Some tests failed. Please review the output above." -ForegroundColor Red
    Write-Host "💡 Common issues to check:" -ForegroundColor Yellow
    Write-Host "   - Backend API compatibility"
    Write-Host "   - Network connectivity"
    Write-Host "   - Request/response format mismatches"
    Write-Host "   - Database schema alignment"
    exit 1
}

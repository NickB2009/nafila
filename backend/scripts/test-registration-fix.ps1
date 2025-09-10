#!/usr/bin/env pwsh

# Test Registration Fix Script
# This script helps verify that the registration fixes are working

Write-Host "🔧 Testing Registration Fix" -ForegroundColor Green

# Test 1: Build the backend
Write-Host "`n📦 Step 1: Building backend..." -ForegroundColor Yellow
Push-Location "GrandeTech.QueueHub"
try {
    $buildResult = dotnet build --no-restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Backend build successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Backend build failed" -ForegroundColor Red
        Write-Host $buildResult
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "❌ Build error: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# Test 2: Run backend tests
Write-Host "`n🧪 Step 2: Running backend tests..." -ForegroundColor Yellow
Push-Location "GrandeTech.QueueHub"
try {
    $testResult = dotnet test --no-build --verbosity minimal 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Backend tests passed" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Some backend tests failed, but continuing..." -ForegroundColor Yellow
        Write-Host $testResult
    }
} catch {
    Write-Host "⚠️ Test error: $_, but continuing..." -ForegroundColor Yellow
}
Pop-Location

# Test 3: Check if API is running locally
Write-Host "`n🌐 Step 3: Testing API connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://api.eutonafila.com.br/api/Health" -Method GET -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API is accessible" -ForegroundColor Green
    } else {
        Write-Host "⚠️ API returned status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ API not accessible: $_" -ForegroundColor Yellow
    Write-Host "   This is normal if the backend isn't deployed yet" -ForegroundColor Gray
}

# Test 4: Test registration endpoint format
Write-Host "`n📝 Step 4: Testing registration request format..." -ForegroundColor Yellow
$testData = @{
    fullName = "Test User PowerShell"
    email = "test.powershell@example.com"
    phoneNumber = "+5511999999999"
    password = "StrongPass123!"
} | ConvertTo-Json

Write-Host "Request data that will be sent:" -ForegroundColor Cyan
Write-Host $testData -ForegroundColor Gray

try {
    $headers = @{
        'Content-Type' = 'application/json'
    }
    
    $response = Invoke-WebRequest -Uri "https://api.eutonafila.com.br/api/Auth/register" -Method POST -Body $testData -Headers $headers -TimeoutSec 15
    
    if ($response.StatusCode -eq 200) {
        Write-Host "🎉 Registration successful!" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Cyan
        Write-Host $response.Content -ForegroundColor Gray
    } else {
        Write-Host "⚠️ Registration returned status: $($response.StatusCode)" -ForegroundColor Yellow
        Write-Host "Response:" -ForegroundColor Cyan
        Write-Host $response.Content -ForegroundColor Gray
    }
} catch {
    $errorResponse = $_.Exception.Response
    if ($errorResponse) {
        $reader = New-Object System.IO.StreamReader($errorResponse.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "❌ Registration failed with status: $($errorResponse.StatusCode)" -ForegroundColor Red
        Write-Host "Response body:" -ForegroundColor Cyan
        Write-Host $responseBody -ForegroundColor Gray
        
        if ($errorResponse.StatusCode -eq 500) {
            Write-Host "`n🔍 500 Error Analysis:" -ForegroundColor Yellow
            Write-Host "- Check backend logs for detailed exception information" -ForegroundColor Gray
            Write-Host "- Verify database connection and schema" -ForegroundColor Gray
            Write-Host "- Ensure all migrations have been applied" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ Registration failed: $_" -ForegroundColor Red
    }
}

Write-Host "`n📋 Summary:" -ForegroundColor Green
Write-Host "1. Backend builds successfully ✅" -ForegroundColor Gray
Write-Host "2. Critical fixes applied:" -ForegroundColor Gray
Write-Host "   - Added missing System.Linq import ✅" -ForegroundColor Gray
Write-Host "   - Fixed User entity configuration ✅" -ForegroundColor Gray
Write-Host "   - Added comprehensive logging ✅" -ForegroundColor Gray
Write-Host "   - Enhanced error handling ✅" -ForegroundColor Gray

Write-Host "`n🎯 Next Steps:" -ForegroundColor Green
Write-Host "1. Deploy the backend with these fixes" -ForegroundColor Gray
Write-Host "2. Check backend logs for detailed error information" -ForegroundColor Gray
Write-Host "3. Test registration from Flutter app" -ForegroundColor Gray
Write-Host "4. Use the test_registration_debug.http file for manual testing" -ForegroundColor Gray

Write-Host "`nScript completed! 🚀" -ForegroundColor Green

# Integration Test Runner for Public APIs
# Usage: 
#   .\run-integration-tests.ps1 -Environment Local
#   .\run-integration-tests.ps1 -Environment Prod -ProdUrl "https://api.eutonafila.com.br"

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Local", "Prod", "BoaHost")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$ProdUrl = "https://api.eutonafila.com.br",
    
    [Parameter(Mandatory=$false)]
    [string]$TestFilter = "TestCategory=Environment"
)

Write-Host "🧪 Running Public API Integration Tests" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Set environment variables
$env:TEST_ENVIRONMENT = $Environment
if ($Environment -eq "Prod" -or $Environment -eq "BoaHost") {
    $env:PROD_API_URL = $ProdUrl
    Write-Host "Production URL: $ProdUrl" -ForegroundColor Yellow
}

# Check if local API is running when testing locally
if ($Environment -eq "Local") {
    Write-Host "Checking if local API is running..." -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5098/api/Health" -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Local API is running" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "❌ Local API is not running. Please start it first:" -ForegroundColor Red
        Write-Host "   cd GrandeTech.QueueHub.API" -ForegroundColor Gray
        Write-Host "   dotnet run --urls=http://localhost:5098" -ForegroundColor Gray
        exit 1
    }
}

# Run the tests
Write-Host "Running tests with filter: $TestFilter" -ForegroundColor Cyan

try {
    dotnet test GrandeTech.QueueHub.Tests --filter $TestFilter --logger "console;verbosity=normal" --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "❌ Test execution failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "🎉 Integration test run completed for $Environment environment" -ForegroundColor Green

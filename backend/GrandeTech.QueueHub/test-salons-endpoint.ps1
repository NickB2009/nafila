# Simple test script to debug the public salons endpoint
Write-Host "Testing /api/Public/salons endpoint..." -ForegroundColor Green

try {
    # Test the endpoint
    $response = Invoke-WebRequest -Uri "http://localhost:5098/api/Public/salons" -UseBasicParsing -ErrorAction Stop
    
    Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Yellow
    Write-Host "Content Type: $($response.Headers.'Content-Type')" -ForegroundColor Yellow
    
    # Parse and display the JSON response
    $jsonContent = $response.Content | ConvertFrom-Json
    
    if ($jsonContent -is [array]) {
        Write-Host "Response is an array with $($jsonContent.Count) items" -ForegroundColor Green
        foreach ($salon in $jsonContent) {
            Write-Host "Salon: $($salon.name) - ID: $($salon.id)" -ForegroundColor Cyan
        }
    } elseif ($jsonContent.errors) {
        Write-Host "Error Response:" -ForegroundColor Red
        foreach ($error in $jsonContent.errors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
    } else {
        Write-Host "Unexpected response format:" -ForegroundColor Yellow
        Write-Host $response.Content -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "Error calling endpoint: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nTesting health endpoint for comparison..." -ForegroundColor Green
try {
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:5098/api/Health" -UseBasicParsing -ErrorAction Stop
    Write-Host "Health endpoint works - Status: $($healthResponse.StatusCode)" -ForegroundColor Green
    Write-Host "Health response: $($healthResponse.Content)" -ForegroundColor Green
} catch {
    Write-Host "Health endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

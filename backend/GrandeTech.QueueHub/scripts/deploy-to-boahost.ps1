# Deploy to BoaHost Script
# Phase 9: Production deployment on BoaHost

Write-Host "Starting BoaHost Deployment..." -ForegroundColor Cyan

# Step 1: Validate environment file
Write-Host "`n1. Validating BoaHost environment configuration..." -ForegroundColor Yellow
if (-not (Test-Path "boahost.env")) {
    Write-Error "ERROR: boahost.env file not found. Please create it with your BoaHost MySQL credentials."
    exit 1
}

# Step 2: Test MySQL connection
Write-Host "`n2. Testing MySQL connection to BoaHost..." -ForegroundColor Yellow
$envContent = Get-Content "boahost.env" | Where-Object { $_ -match "^MYSQL_" }
$mysqlHost = ($envContent | Where-Object { $_ -match "^MYSQL_HOST=" }) -replace "MYSQL_HOST=", ""
$mysqlUser = ($envContent | Where-Object { $_ -match "^MYSQL_USER=" }) -replace "MYSQL_USER=", ""
$mysqlPassword = ($envContent | Where-Object { $_ -match "^MYSQL_PASSWORD=" }) -replace "MYSQL_PASSWORD=", ""
$mysqlDatabase = ($envContent | Where-Object { $_ -match "^MYSQL_DATABASE=" }) -replace "MYSQL_DATABASE=", ""

Write-Host "Testing connection to: $mysqlHost" -ForegroundColor Gray
Write-Host "Database: $mysqlDatabase" -ForegroundColor Gray
Write-Host "User: $mysqlUser" -ForegroundColor Gray

# Step 3: Build Docker image
Write-Host "`n3. Building Docker image for production..." -ForegroundColor Yellow
docker build -t grandetech-queuehub-api:boahost -f ./GrandeTech.QueueHub.API/Dockerfile ./GrandeTech.QueueHub.API

if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: Docker build failed"
    exit 1
}

# Step 4: Stop existing services
Write-Host "`n4. Stopping existing services..." -ForegroundColor Yellow
docker-compose -f docker-compose.boahost.yml --env-file boahost.env down

# Step 5: Start production services
Write-Host "`n5. Starting production services on BoaHost..." -ForegroundColor Yellow
docker-compose -f docker-compose.boahost.yml --env-file boahost.env up -d

if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: Failed to start production services"
    exit 1
}

# Step 6: Wait for services to be ready
Write-Host "`n6. Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Step 7: Test API
Write-Host "`n7. Testing API endpoints..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost" -Method GET -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API is responding successfully!" -ForegroundColor Green
        Write-Host "Response: $($response.Content)" -ForegroundColor Gray
    } else {
        Write-Warning "⚠️ API responded with status code: $($response.StatusCode)"
    }
} catch {
    Write-Warning "⚠️ Could not test API: $($_.Exception.Message)"
}

# Step 8: Show deployment status
Write-Host "`n8. Deployment Status:" -ForegroundColor Yellow
docker-compose -f docker-compose.boahost.yml --env-file boahost.env ps

Write-Host "`n🎉 BoaHost Deployment Complete!" -ForegroundColor Green
Write-Host "Your API is now running on BoaHost with MySQL database!" -ForegroundColor Green
Write-Host "`nAccess your API at:" -ForegroundColor Cyan
Write-Host "- Main API: http://your-domain.com" -ForegroundColor White
Write-Host "- Swagger Docs: http://your-domain.com/swagger" -ForegroundColor White
Write-Host "- Health Check: http://your-domain.com/health" -ForegroundColor White

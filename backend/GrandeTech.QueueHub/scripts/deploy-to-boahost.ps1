# Deploy to BoaHost Script (Standalone - No Docker)
# Phase 9: Production deployment on BoaHost

Write-Host "Starting BoaHost Standalone Deployment..." -ForegroundColor Cyan

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

# Step 3: Build .NET application
Write-Host "`n3. Building .NET application for production..." -ForegroundColor Yellow
dotnet publish ./GrandeTech.QueueHub.API/GrandeTech.QueueHub.API.csproj -c Release -o ./publish --self-contained true -r linux-x64

if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: .NET build failed"
    exit 1
}

# Step 4: Stop existing service
Write-Host "`n4. Stopping existing service..." -ForegroundColor Yellow
if (Get-Service -Name "queuehub-api" -ErrorAction SilentlyContinue) {
    Stop-Service -Name "queuehub-api" -Force
    Write-Host "Stopped existing service" -ForegroundColor Green
}

# Step 5: Copy application files
Write-Host "`n5. Copying application files..." -ForegroundColor Yellow
$deployPath = "/var/www/queuehub-api"
if (-not (Test-Path $deployPath)) {
    New-Item -ItemType Directory -Path $deployPath -Force
}

Copy-Item -Path "./publish/*" -Destination $deployPath -Recurse -Force
Copy-Item -Path "./GrandeTech.QueueHub.API/final_database_seeding.sql" -Destination "$deployPath/" -Force

# Step 6: Set up database
Write-Host "`n6. Setting up database..." -ForegroundColor Yellow
if (Get-Command mysql -ErrorAction SilentlyContinue) {
    mysql -h $mysqlHost -u $mysqlUser -p$mysqlPassword -P 3306 $mysqlDatabase -e "SOURCE $deployPath/final_database_seeding.sql;"
    Write-Host "Database seeded successfully" -ForegroundColor Green
} else {
    Write-Warning "MySQL client not found. Please run the seeding script manually:"
    Write-Host "mysql -h $mysqlHost -u $mysqlUser -p$mysqlPassword -P 3306 $mysqlDatabase < $deployPath/final_database_seeding.sql" -ForegroundColor Yellow
}

# Step 7: Create systemd service
Write-Host "`n7. Creating systemd service..." -ForegroundColor Yellow
$serviceContent = @"
[Unit]
Description=GrandeTech QueueHub API
After=network.target

[Service]
Type=notify
ExecStart=$deployPath/GrandeTech.QueueHub.API
WorkingDirectory=$deployPath
Restart=always
RestartSec=10
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:80

[Install]
WantedBy=multi-user.target
"@

$serviceContent | Out-File -FilePath "/etc/systemd/system/queuehub-api.service" -Encoding UTF8

# Step 8: Start service
Write-Host "`n8. Starting service..." -ForegroundColor Yellow
systemctl daemon-reload
systemctl enable queuehub-api
systemctl start queuehub-api

# Step 9: Wait for service to be ready
Write-Host "`n9. Waiting for service to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Step 10: Test API
Write-Host "`n10. Testing API endpoints..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost/health" -Method GET -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API is responding successfully!" -ForegroundColor Green
        Write-Host "Response: $($response.Content)" -ForegroundColor Gray
    } else {
        Write-Warning "⚠️ API responded with status code: $($response.StatusCode)"
    }
} catch {
    Write-Warning "⚠️ Could not test API: $($_.Exception.Message)"
}

# Step 11: Show service status
Write-Host "`n11. Service Status:" -ForegroundColor Yellow
systemctl status queuehub-api --no-pager

Write-Host "`n🎉 BoaHost Standalone Deployment Complete!" -ForegroundColor Green
Write-Host "Your API is now running as a standalone service on BoaHost!" -ForegroundColor Green
Write-Host "`nAccess your API at:" -ForegroundColor Cyan
Write-Host "- Main API: http://your-domain.com" -ForegroundColor White
Write-Host "- Swagger Docs: http://your-domain.com/swagger" -ForegroundColor White
Write-Host "- Health Check: http://your-domain.com/health" -ForegroundColor White
Write-Host "`nService management commands:" -ForegroundColor Cyan
Write-Host "- Start: systemctl start queuehub-api" -ForegroundColor White
Write-Host "- Stop: systemctl stop queuehub-api" -ForegroundColor White
Write-Host "- Status: systemctl status queuehub-api" -ForegroundColor White
Write-Host "- Logs: journalctl -u queuehub-api -f" -ForegroundColor White

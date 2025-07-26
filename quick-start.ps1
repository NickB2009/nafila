#!/usr/bin/env pwsh
# Quick Start Script for EuToNaFila QueueHub
# This script gets you up and running with Docker, SQL Server, and fake data in minutes!

Write-Host "🚀 EuToNaFila QueueHub Quick Start" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Check if Docker is running
Write-Host "🔍 Checking Docker..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "✅ Docker is running!" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Start SQL Server in Docker
Write-Host ""
Write-Host "🐳 Starting SQL Server container..." -ForegroundColor Yellow
docker run -d --name nafila-sqlserver `
    -e "ACCEPT_EULA=Y" `
    -e "SA_PASSWORD=DevPassword123!" `
    -e "MSSQL_PID=Developer" `
    -p 1433:1433 `
    mcr.microsoft.com/mssql/server:2019-latest

if ($LASTEXITCODE -ne 0) {
    Write-Host "Trying to restart existing container..." -ForegroundColor Yellow
    docker start nafila-sqlserver
}

# Wait for SQL Server to be ready
Write-Host "⏳ Waiting for SQL Server to be ready..." -ForegroundColor Yellow
$timeout = 30
$elapsed = 0
do {
    Start-Sleep -Seconds 2
    $elapsed += 2
    $isReady = docker exec nafila-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "DevPassword123!" -Q "SELECT 1" 2>$null
} while ($LASTEXITCODE -ne 0 -and $elapsed -lt $timeout)

if ($elapsed -ge $timeout) {
    Write-Host "❌ SQL Server failed to start within $timeout seconds" -ForegroundColor Red
    exit 1
}

Write-Host "✅ SQL Server is ready!" -ForegroundColor Green

# Navigate to backend directory and run the API
Write-Host ""
Write-Host "🏗️ Building and starting the API..." -ForegroundColor Yellow
Set-Location "backend/GrandeTech.QueueHub/GrandeTech.QueueHub.API"

Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build

Write-Host "🎯 Starting API with automatic database setup..." -ForegroundColor Yellow
Write-Host ""
Write-Host "🎉 The API will now:" -ForegroundColor Cyan
Write-Host "   • Connect to SQL Server" -ForegroundColor White
Write-Host "   • Run database migrations" -ForegroundColor White
Write-Host "   • Seed with awesome fake data:" -ForegroundColor White
Write-Host "     - Downtown Barbers & Healthy Life Clinic" -ForegroundColor Gray
Write-Host "     - Clark Kent, Lois Lane, Bruce Wayne, Diana Prince" -ForegroundColor Gray
Write-Host "     - Realistic business data and branding" -ForegroundColor Gray
Write-Host ""
Write-Host "🌐 API will be available at:" -ForegroundColor Cyan
Write-Host "   • HTTP: http://localhost:5098" -ForegroundColor White
Write-Host "   • HTTPS: https://localhost:7126" -ForegroundColor White
Write-Host "   • Swagger: https://localhost:7126/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop the API when you're done." -ForegroundColor Yellow
Write-Host ""

# Run the API
dotnet run 
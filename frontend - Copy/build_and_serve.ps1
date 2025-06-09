#!/usr/bin/env pwsh

Write-Host "🏗️  Building Flutter Web App for Production..." -ForegroundColor Green
Write-Host ""

# Change to the frontend directory
Set-Location -Path $PSScriptRoot

# Clean previous build
Write-Host "🧹 Cleaning previous build..." -ForegroundColor Yellow
flutter clean
flutter pub get

# Build for web
Write-Host "🔨 Building web app..." -ForegroundColor Yellow
flutter build web --web-renderer html --release

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    # Find available port
    $port = 8080
    while ((Test-NetConnection -ComputerName localhost -Port $port -InformationLevel Quiet -WarningAction SilentlyContinue)) {
        $port++
        if ($port -gt 8090) {
            Write-Host "❌ No available ports found between 8080-8090" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "🌐 Serving at: http://localhost:$port" -ForegroundColor Cyan
    Write-Host "🛑 Press Ctrl+C to stop the server" -ForegroundColor Red
    Write-Host ""
    
    # Serve the built app
    Set-Location "build\web"
    python -m http.server $port
} else {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

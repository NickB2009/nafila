#!/usr/bin/env pwsh

Write-Host "🚀 Starting Flutter Web Development..." -ForegroundColor Green
Write-Host ""

# Change to the frontend directory
Set-Location -Path $PSScriptRoot

# Clean any existing processes
Write-Host "🧹 Cleaning existing processes..." -ForegroundColor Yellow
Get-Process -Name "flutter" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dart" -ErrorAction SilentlyContinue | Stop-Process -Force

# Find available port
$port = 8080
while ((Test-NetConnection -ComputerName localhost -Port $port -InformationLevel Quiet -WarningAction SilentlyContinue)) {
    $port++
    if ($port -gt 8090) {
        Write-Host "❌ No available ports found between 8080-8090" -ForegroundColor Red
        exit 1
    }
}

Write-Host "🔧 Using port: $port" -ForegroundColor Cyan
Write-Host "🌐 App will be available at: http://localhost:$port" -ForegroundColor Green
Write-Host "⚡ Hot reload enabled - save files to see changes" -ForegroundColor Yellow
Write-Host "🛑 Press Ctrl+C to stop the server" -ForegroundColor Red
Write-Host ""

# Start Flutter development server
try {
    flutter run -d chrome --web-renderer html --web-port $port
} catch {
    Write-Host "❌ Failed to start Flutter development server" -ForegroundColor Red
    Write-Host "💡 Trying alternative approach..." -ForegroundColor Yellow
    
    # Build and serve as fallback
    flutter build web --web-renderer html
    Set-Location "build\web"
    python -m http.server $port
}

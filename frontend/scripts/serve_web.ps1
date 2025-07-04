# Serve Flutter Web App
Write-Host "🌐 Building Flutter Web App..." -ForegroundColor Green

# Build the app
flutter build web --release

Write-Host "🚀 Starting Web Server..." -ForegroundColor Yellow

# Serve the built app
cd build/web
python -m http.server 8080

Write-Host "✅ App running at http://localhost:8080" -ForegroundColor Green 
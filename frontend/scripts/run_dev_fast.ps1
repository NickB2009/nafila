# Fast Flutter Development Script
Write-Host "🚀 Starting Fast Flutter Development..." -ForegroundColor Green

# Clean and get dependencies
Write-Host "📦 Cleaning and getting dependencies..." -ForegroundColor Yellow
flutter clean
flutter pub get

# Run with optimizations
Write-Host "⚡ Running Flutter with optimizations..." -ForegroundColor Yellow
flutter run -d chrome --release --no-hot --web-port 8080

Write-Host "✅ App should be running at http://localhost:8080" -ForegroundColor Green 
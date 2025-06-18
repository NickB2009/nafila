@echo off
echo 🚀 Starting Fast Flutter Development...

echo 📦 Cleaning and getting dependencies...
flutter clean
flutter pub get

echo ⚡ Running Flutter with optimizations...
flutter run -d chrome --release --no-hot --web-port 8080

echo ✅ App should be running at http://localhost:8080
pause 
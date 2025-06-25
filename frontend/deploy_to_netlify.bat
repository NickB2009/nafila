@echo off
echo ========================================
echo    Deploying Flutter App to Netlify
echo    (Public Access - No Login Required)
echo ========================================
echo.

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Node.js is not installed!
    echo Please install Node.js from: https://nodejs.org/
    pause
    exit /b 1
)

REM Check if Netlify CLI is installed
netlify --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Installing Netlify CLI...
    npm install -g netlify-cli
    if %errorlevel% neq 0 (
        echo Error: Failed to install Netlify CLI!
        pause
        exit /b 1
    )
)

REM Build the Flutter web app
echo Building Flutter web app...
flutter build web --release
if %errorlevel% neq 0 (
    echo Error: Flutter build failed!
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.

REM Deploy to Netlify
echo Deploying to Netlify...
netlify deploy --dir=build/web --prod

echo.
echo ========================================
echo    Deployment completed!
echo ========================================
echo.
echo Your app is now publicly accessible!
echo Users can:
echo - Visit the URL without any login
echo - Install it as a PWA to their device
echo - Use it offline after first visit
echo.
pause 
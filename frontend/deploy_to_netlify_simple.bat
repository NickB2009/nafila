@echo off
echo ========================================
echo    Deploying Flutter App to Netlify
echo    (Simple Method - No CLI Issues)
echo ========================================
echo.

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

REM Open the build folder
echo Opening build folder...
start build\web

echo.
echo ========================================
echo    NEXT STEPS:
echo ========================================
echo 1. Go to: https://app.netlify.com/
echo 2. Sign up/Login to Netlify
echo 3. Drag and drop the 'build\web' folder
echo    (or the entire contents of the folder)
echo 4. Wait for deployment to complete
echo 5. Get your public URL!
echo.
echo Your app will be:
echo - Publicly accessible (no login required)
echo - PWA ready (users can install it)
echo - Free forever
echo.
echo Press any key to open Netlify...
pause > nul
start https://app.netlify.com/ 
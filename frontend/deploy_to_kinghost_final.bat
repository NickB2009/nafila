@echo off
echo ========================================
echo    Deploying Flutter App to KingHost
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

REM Create _redirects file for proper routing
echo Creating _redirects file for KingHost...
echo /*    /index.html   200 > build\web\_redirects

REM Open the build folder
echo Opening build folder...
start build\web

echo.
echo ========================================
echo    NEXT STEPS FOR KINGHOST:
echo ========================================
echo 1. Go to your KingHost control panel
echo 2. Open File Manager or use FTP
echo 3. Navigate to: public_html
echo 4. Upload ALL files from build\web folder
echo.
echo OR use FTP:
echo - Host: ftp.eutonafila.com.br (or your FTP host)
echo - Username: Your KingHost FTP username
echo - Password: Your KingHost FTP password
echo - Directory: public_html
echo.
echo Your app will be available at:
echo - https://eutonafila.com.br (if domain is configured)
echo - Or your KingHost default URL
echo.
echo Press any key to continue...
pause > nul 
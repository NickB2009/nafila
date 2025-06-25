@echo off
echo ========================================
echo    Deploying Flutter App to KingHost
echo    (Default URL - No Domain Setup)
echo ========================================
echo.

REM Build the Flutter web app first
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

REM Create FTP script
echo Creating FTP script...
(
echo open YOUR_KINGHOST_FTP_HOST
echo user YOUR_FTP_USERNAME YOUR_FTP_PASSWORD
echo binary
echo cd public_html
echo lcd build\web
echo mput *
echo bye
) > ftpscript.txt

echo.
echo ========================================
echo    NEXT STEPS:
echo ========================================
echo 1. Edit ftpscript.txt and replace:
echo    - YOUR_KINGHOST_FTP_HOST with your FTP host
echo    - YOUR_FTP_USERNAME with your username
echo    - YOUR_FTP_PASSWORD with your password
echo.
echo 2. Run: ftp -s:ftpscript.txt
echo.
echo 3. Your app will be available at:
echo    https://YOUR_KINGHOST_DEFAULT_URL
echo.
echo Common KingHost URLs:
echo - yourusername.kinghost.net
echo - yourdomain.kinghost.net
echo - Check your KingHost control panel
echo.
pause 
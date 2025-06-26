@echo off
echo ========================================
echo    Deploying Flutter App to KingHost
echo    (FTP Method - Domain Ready)
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

REM Get FTP credentials
echo Please enter your KingHost FTP credentials:
set /p FTP_USERNAME="FTP Username: "
set /p FTP_PASSWORD="FTP Password: "

REM Create FTP script
echo Creating FTP script...
(
echo open ftp.eutonafila.com.br
echo user %FTP_USERNAME% %FTP_PASSWORD%
echo binary
echo cd public_html
echo lcd build\web
echo mput *
echo bye
) > ftpscript.txt

echo.
echo Starting FTP upload...
ftp -s:ftpscript.txt

REM Clean up
del ftpscript.txt

echo.
echo ========================================
echo    Deployment completed!
echo ========================================
echo.
echo Your app should be available at:
echo https://eutonafila.com.br
echo.
pause 
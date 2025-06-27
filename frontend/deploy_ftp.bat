@echo off
setlocal EnableDelayedExpansion

echo ========================================
echo    Uploading Flutter App to KingHost
echo ========================================
echo.

:: Check if build directory exists
if not exist "build\web" (
    echo [ERROR] Build directory not found!
    echo Please run: flutter build web --release
    pause
    exit /b 1
)

:: Get FTP credentials
echo Please enter your FTP credentials:
set /p "username=Username: "
set /p "password=Password: "

:: Create FTP commands file
echo [INFO] Creating FTP commands...
set "ftp_commands=ftp_commands.txt"

:: Write FTP commands with login
(
    echo open ftp.eutonafila.com.br
    echo %username%
    echo %password%
    echo binary
    echo cd /
    echo lcd "build\web"
    echo put test.html
    echo quit
) > %ftp_commands%

echo [INFO] First, let's test with test.html...

:: Run FTP command
ftp -n -s:%ftp_commands% -i

:: Clean up sensitive data
del %ftp_commands%

echo.
echo [INFO] Testing if test.html was uploaded...
echo Please visit: http://eutonafila.com.br/test.html
echo.
set /p "continue=Did the test page load successfully? (Y/N): "
if /i not "%continue%"=="Y" (
    echo [ERROR] Test upload failed. Please check your FTP credentials and try again.
    pause
    exit /b 1
)

echo.
echo [INFO] Test successful! Now uploading all Flutter files...
echo.

:: Create new FTP commands for full upload
(
    echo open ftp.eutonafila.com.br
    echo %username%
    echo %password%
    echo binary
    echo cd /
    echo lcd "build\web"
    echo delete index.htm
    echo delete index.html
    echo put index.html
    echo put flutter.js
    echo put main.dart.js
    echo put flutter_service_worker.js
    echo put flutter_bootstrap.js
    echo put manifest.json
    echo put favicon.png
    echo put version.json
    echo mkdir assets
    echo cd assets
    echo lcd "assets"
    echo mput *.*
    echo cd ..
    echo lcd ..
    echo mkdir canvaskit
    echo cd canvaskit
    echo lcd "canvaskit"
    echo mput *.*
    echo cd ..
    echo lcd ..
    echo mkdir icons
    echo cd icons
    echo lcd "icons"
    echo mput *.*
    echo quit
) > %ftp_commands%

:: Run FTP command for full upload
ftp -n -s:%ftp_commands% -i

:: Clean up
del %ftp_commands%

echo.
echo ========================================
echo          DEPLOYMENT COMPLETE!
echo ========================================
echo.
echo Please check:
echo 1. http://eutonafila.com.br/test.html
echo 2. http://eutonafila.com.br
echo.
echo If you see the construction page:
echo - Try clearing your browser cache
echo - Try a different browser
echo - Check if all files were uploaded correctly
echo.
pause 
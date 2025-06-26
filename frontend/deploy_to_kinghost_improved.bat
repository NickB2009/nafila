@echo off
setlocal enabledelayedexpansion

REM Flutter App Deployment Script for KingHost (Windows)
REM Usage: deploy_to_kinghost_improved.bat

REM Configuration
set HOST=ftp.eutonafila.com.br
set PORT=21
set REMOTE_DIR=/public_html
set LOCAL_BUILD_DIR=build\web
set DOMAIN=eutonafila.com.br

echo ========================================
echo    Deploying Flutter App to KingHost
echo ========================================
echo.

REM Check if Flutter is installed
flutter --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Flutter is not installed or not in PATH
    echo [ERROR] Please install Flutter: https://docs.flutter.dev/get-started/install
    pause
    exit /b 1
)

echo [INFO] Flutter installation verified
echo.

REM Get FTP credentials
echo [INFO] Please enter your KingHost FTP credentials:
set /p FTP_USER="FTP Username: "
set /p FTP_PASS="FTP Password: "

if "%FTP_USER%"=="" (
    echo [ERROR] Username is required
    pause
    exit /b 1
)

if "%FTP_PASS%"=="" (
    echo [ERROR] Password is required
    pause
    exit /b 1
)

echo.

REM Test DNS resolution
echo [INFO] Testing DNS resolution for %HOST%...
nslookup %HOST% >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] DNS resolution test failed, but continuing...
) else (
    echo [INFO] DNS resolution successful
)
echo.

REM Test FTP connection
echo [INFO] Testing FTP connection...
(
echo open %HOST% %PORT%
echo user %FTP_USER% %FTP_PASS%
echo pwd
echo bye
) > test_ftp.txt

ftp -inv < test_ftp.txt >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] FTP connection test failed
    del test_ftp.txt
    pause
    exit /b 1
)

echo [INFO] FTP connection test successful
del test_ftp.txt
echo.

REM Build the Flutter web app
echo [INFO] Building Flutter web app...
echo [INFO] Cleaning previous build...
flutter clean

echo [INFO] Building for web release...
flutter build web --release
if %errorlevel% neq 0 (
    echo [ERROR] Flutter build failed!
    pause
    exit /b 1
)

echo [INFO] Build completed successfully!
echo.

REM Verify build directory exists
if not exist "%LOCAL_BUILD_DIR%" (
    echo [ERROR] Build directory %LOCAL_BUILD_DIR% not found!
    pause
    exit /b 1
)

echo [INFO] Build files ready in %LOCAL_BUILD_DIR%
echo.

REM Create FTP script
echo [INFO] Creating FTP upload script...
(
echo open %HOST% %PORT%
echo user %FTP_USER% %FTP_PASS%
echo binary
echo cd %REMOTE_DIR%
echo lcd %LOCAL_BUILD_DIR%
echo mput *
echo bye
) > ftpscript.txt

echo [INFO] FTP script created
echo.

REM Upload files
echo [INFO] Starting FTP upload to %HOST%...
ftp -inv < ftpscript.txt
if %errorlevel% neq 0 (
    echo [ERROR] FTP upload failed!
    del ftpscript.txt
    pause
    exit /b 1
)

echo [INFO] Upload completed successfully!
echo.

REM Cleanup
echo [INFO] Cleaning up temporary files...
del ftpscript.txt

REM Success message
echo ========================================
echo    Deployment completed successfully!
echo ========================================
echo.
echo [INFO] Your app should be available at: https://%DOMAIN%
echo [INFO] Please allow a few minutes for changes to propagate.
echo.
pause 
@echo off
echo ========================================
echo    Deploying Flutter App to KingHost
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
echo open ftp.eutonafila.com.br
echo user YOUR_FTP_USERNAME YOUR_FTP_PASSWORD
echo binary
echo cd public_html
echo lcd build\web
echo mput *
echo bye
) > ftpscript.txt

echo.
echo FTP script created. Please edit ftpscript.txt to add your credentials:
echo - Replace YOUR_FTP_USERNAME with your KingHost FTP username
echo - Replace YOUR_FTP_PASSWORD with your KingHost FTP password
echo.
echo Then run: ftp -s:ftpscript.txt
echo.
echo Or press any key to run FTP now (if credentials are already set)...
pause > nul

REM Run FTP
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
echo Your app should be available at: https://eutonafila.com.br
echo.
pause 
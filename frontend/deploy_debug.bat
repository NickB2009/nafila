@echo off
echo ===========================================
echo      NAFILA DEBUG DEPLOYMENT SCRIPT
echo ===========================================
echo.

:: Check if build directory exists
if not exist "build\web" (
    echo ERROR: Build directory not found!
    echo Please run: flutter build web --release
    pause
    exit /b 1
)

echo 1. Building Flutter web app...
call flutter build web --release
if %ERRORLEVEL% neq 0 (
    echo ERROR: Flutter build failed!
    pause
    exit /b 1
)

echo.
echo 2. Build completed successfully!
echo.
echo Debug files ready for upload:
echo - build\web\index.html (main app)
echo - build\web\test.html (debug page)
echo - build\web\*.js (Flutter files)
echo - build\web\assets\ (Flutter assets)
echo.

echo ===========================================
echo         MANUAL UPLOAD INSTRUCTIONS
echo ===========================================
echo.
echo Upload these files to your KingHost FTP:
echo Server: ftp.eutonafila.com.br
echo Directory: / (root directory, same level as current index.htm)
echo.
echo FILES TO UPLOAD:
echo ----------------
echo FROM build\web\ folder, upload ALL files:
echo - index.html
echo - test.html
echo - flutter.js
echo - main.dart.js
echo - flutter_service_worker.js
echo - flutter_bootstrap.js
echo - manifest.json
echo - favicon.png
echo - version.json
echo - _redirects
echo - assets\ (entire folder)
echo - canvaskit\ (entire folder)  
echo - icons\ (entire folder)
echo.
echo TESTING STEPS:
echo --------------
echo 1. Upload all files
echo 2. Visit: http://eutonafila.com.br/test.html
echo 3. If test page works, visit: http://eutonafila.com.br
echo 4. Check browser console (F12) for any errors
echo.

echo Would you like to open the build folder? (Y/N)
set /p open_folder=
if /i "%open_folder%"=="Y" (
    start explorer "build\web"
)

echo.
echo ===========================================
echo    TROUBLESHOOTING TIPS
echo ===========================================
echo.
echo If still seeing "Página em construção":
echo.
echo 1. CHECK INDEX FILES:
echo    - Make sure index.html is uploaded (not index.htm)
echo    - Delete any old index.htm file
echo.
echo 2. CACHE ISSUES:
echo    - Try different browser
echo    - Hard refresh: Ctrl+F5
echo    - Clear browser cache
echo.
echo 3. FILE PERMISSIONS:
echo    - Check if files are readable on server
echo    - Try uploading in binary mode
echo.
echo 4. PATH ISSUES:
echo    - Make sure files are in root directory
echo    - Check if there's a .htaccess file interfering
echo.
echo 5. SERVER CONFIGURATION:
echo    - KingHost might have index.php or default.html priority
echo    - Contact KingHost support about index file priority
echo.

pause 
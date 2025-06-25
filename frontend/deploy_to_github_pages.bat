@echo off
echo ========================================
echo    Deploying Flutter App to GitHub Pages
echo    (Alternative to Netlify)
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

REM Check if git is initialized
if not exist .git (
    echo Initializing git repository...
    git init
    git add .
    git commit -m "Initial commit"
)

echo.
echo ========================================
echo    NEXT STEPS:
echo ========================================
echo 1. Create a new repository on GitHub
echo 2. Push your code to GitHub:
echo    git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
echo    git push -u origin main
echo.
echo 3. Enable GitHub Pages:
echo    - Go to your repository on GitHub
echo    - Settings > Pages
echo    - Source: Deploy from a branch
echo    - Branch: main
echo    - Folder: / (root)
echo.
echo 4. Your app will be available at:
echo    https://YOUR_USERNAME.github.io/YOUR_REPO/
echo.
echo Press any key to open GitHub...
pause > nul
start https://github.com/new 
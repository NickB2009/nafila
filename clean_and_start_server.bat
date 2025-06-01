@echo off
echo ======================================
echo     CLEAN DATABASE AND FIX UUID
echo ======================================
echo.
echo This script will:
echo 1. Truncate all database tables to clean up any corrupted data
echo 2. Apply the UUID handling patch to fix SQLite's UUID handling
echo 3. Start the server with the fixed setup
echo.
echo Make sure you have a backup of any important data!
echo.

:: Check if Python is installed
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python not found. Please install Python and try again.
    pause
    exit /b 1
)

:: Create and activate virtual environment if it doesn't exist
if not exist venv (
    echo Creating virtual environment...
    python -m venv venv
)

echo Activating virtual environment...
call venv\Scripts\activate.bat

:: Install requirements if needed
echo Installing dependencies...
pip install -r requirements.txt

:: Run the truncation script
echo.
echo Step 1: Truncating database tables...
python truncate_all_tables.py

:: Run the UUID fix script
echo.
echo Step 2: Applying SQLite UUID patch...
python fix_sqlite_uuid.py

:: Apply migrations
echo.
echo Step 3: Applying migrations...
cd eutonafila
python manage.py makemigrations
python manage.py migrate

:: Apply patches to fix UUID and enum issues
echo.
echo Step 4: Applying additional patches...
python -c "from middleware import MonkeyPatchMiddleware; patch = MonkeyPatchMiddleware(lambda x: x); patch.apply_patches()"

:: Run ensure_services to make sure all services are created properly
echo.
echo Step 5: Verifying default services...
python manage.py ensure_services

:: Start the Django server
echo.
echo Step 6: Starting the Django server...
echo.
echo Access http://127.0.0.1:8000/ in your browser
echo.
echo Press Ctrl+C to stop the server
echo.
python manage.py runserver

:: Deactivate virtual environment when done
cd ..
call deactivate
pause

@echo off
echo ======================================
echo      TRUNCATE ALL TABLES SCRIPT
echo ======================================
echo.
echo This script will delete ALL data from the database.
echo It will help to resolve UUID issues with "django.db.backends.sqlite3.operations.DatabaseOperations" objects.
echo.
echo Make sure you have a backup if you need to preserve any data!
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

:: Run the script
echo.
echo Running truncate_all_tables.py...
python truncate_all_tables.py

:: Deactivate virtual environment when done
call deactivate
pause

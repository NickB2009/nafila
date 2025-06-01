@echo off
echo ======================================
echo      FIX SQLITE UUID HANDLING
echo ======================================
echo.
echo This script will patch Django's SQLite backend to properly handle UUID values.
echo It addresses the issue with "🔑 <django.db.backends.sqlite3.operations.DatabaseOperations object>" 
echo appearing instead of proper UUID values.
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

:: Run the UUID fix script
echo.
echo Applying SQLite UUID patch...
python fix_sqlite_uuid.py

:: Deactivate virtual environment when done
call deactivate
pause

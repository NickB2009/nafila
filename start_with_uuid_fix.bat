@echo off
echo ======================================
echo     STARTING SERVER WITH UUID FIX
echo ======================================
echo.

echo 1. Applying SQLite UUID fix...
python simple_uuid_fix.py

echo.
echo 2. Starting the server...
cd eutonafila
python manage.py runserver

pause

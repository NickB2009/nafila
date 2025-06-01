@echo off
echo ======================================
echo     AUTOMATIC UUID FIX SCRIPT
echo ======================================
echo.

:: Run the UUID fix script
python fix_uuid_auto.py

:: If successful, offer to start the server
echo.
echo Do you want to start the server now? (y/n)
set /p start_server=
if /i "%start_server%" == "y" (
    echo.
    echo Starting Django server...
    cd eutonafila
    python manage.py runserver
)

echo.
echo Script completed.
pause

@echo off
echo Applying UUID field patch and starting server...

cd /d "%~dp0"
cd eutonafila

rem Run the server with our patched __init__.py
python manage.py runserver

pause

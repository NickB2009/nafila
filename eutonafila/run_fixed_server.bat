@echo off
echo Running Django server with UUID patch...

cd /d "%~dp0"
cd eutonafila

python manage.py runserver

pause

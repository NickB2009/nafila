@echo off
echo Running UUID fixes for Servico model...
python fix_uuid_servico.py
echo.
echo Starting Django server with fixed UUIDs...
echo.
python eutonafila\manage.py runserver

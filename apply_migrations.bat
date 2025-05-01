@echo off
echo Applying database migrations...
call venv\Scripts\activate.bat
python apply_migrations.py
echo Done!
pause 
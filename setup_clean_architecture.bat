@echo off
echo Setting up Clean Architecture directory structure...
call venv\Scripts\activate.bat
python setup_clean_architecture.py
echo Done!
pause 
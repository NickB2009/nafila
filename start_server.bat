@echo off
echo ======================================
echo    INICIANDO SERVIDOR EU TO NA FILA
echo ======================================
echo.

:: Check if Python is installed
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python nao encontrado. Por favor, instale o Python e tente novamente.
    pause
    exit /b 1
)

:: Create and activate virtual environment if it doesn't exist
if not exist venv (
    echo Criando ambiente virtual...
    python -m venv venv
)

echo Ativando ambiente virtual...
call venv\Scripts\activate.bat

:: Install requirements if needed
echo Instalando dependencias...
pip install -r requirements.txt

:: Change to the correct directory
echo Mudando para o diretorio correto...
cd eutonafila

:: Create and apply migrations
echo.
echo Criando migracoes...
python manage.py makemigrations
echo.
echo Aplicando migracoes...
python manage.py migrate

:: Start the Django server
echo.
echo Iniciando o servidor Django...
echo.
echo Acesse http://127.0.0.1:8000/ no seu navegador
echo.
echo Pressione Ctrl+C para parar o servidor
echo.
python manage.py runserver

:: Deactivate virtual environment when done
call deactivate
pause 
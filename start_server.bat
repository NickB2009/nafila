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

:: Run the UUID fix to solve DB-related issues
echo Aplicando correcoes de UUID...
python fix_uuid.py

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

:: Apply patches to fix UUID and enum issues
echo.
echo Aplicando patches...
python -c "from middleware import MonkeyPatchMiddleware; patch = MonkeyPatchMiddleware(lambda x: x); patch.apply_patches()"

:: Run ensure_services to make sure all services are created properly
echo.
echo Verificando servicos padroes...
python manage.py ensure_services

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
cd ..
call deactivate
pause 
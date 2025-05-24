@echo off
setlocal enabledelayedexpansion

echo ======================================
echo   FIX + START SERVIDOR EU TO NA FILA
echo ======================================
echo.

:: Activate virtual environment
echo Ativando ambiente virtual...
call venv\Scripts\activate.bat

:: Run the UUID fix
echo.
echo 1/4 - Aplicando correcoes de UUID...
python fix_uuid.py

:: Change to the eutonafila directory
cd eutonafila

:: Apply patches
echo.
echo 2/4 - Aplicando middleware patches...
python manage.py shell -c "from middleware import MonkeyPatchMiddleware; patch = MonkeyPatchMiddleware(lambda x: x); patch.apply_patches()"

:: Run the ensure services command
echo.
echo 3/4 - Verificando servicos padroes...
python manage.py ensure_services

:: Run the server
echo.
echo 4/4 - Iniciando o servidor...
echo.
echo Servidor rodando em http://127.0.0.1:8000/
echo Pressione Ctrl+C para parar o servidor
echo.
python manage.py runserver

:: Deactivate virtual environment when done
cd ..
call deactivate

endlocal

@echo off
echo ======================================
echo     CORRIGINDO PROBLEMAS DE UUID
echo ======================================
echo.

:: Activate virtual environment
call venv\Scripts\activate.bat

:: Run the fix script
echo Aplicando correções de UUID...
python fix_uuid.py

:: Run the ensure_services command manually to validate the fix
echo.
echo Executando ensure_services para garantir serviços em todas as barbearias...
cd eutonafila
python manage.py ensure_services
cd ..

echo.
echo Correções aplicadas. Agora pode executar start_server.bat normalmente.
echo.

pause

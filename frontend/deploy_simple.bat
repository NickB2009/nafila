@echo off
echo ========================================
echo    Uploading Flutter App to KingHost
echo ========================================
echo.

cd build\web

echo Please enter your FTP credentials
set /p "username=Username: "
set /p "password=Password: "

echo user %username%> ftpcmd.txt
echo %password%>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd />> ftpcmd.txt
echo put test.html>> ftpcmd.txt
echo quit>> ftpcmd.txt

echo Uploading test.html...
ftp -n -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo Test if the file is accessible at:
echo http://eutonafila.com.br/test.html
echo.
pause 
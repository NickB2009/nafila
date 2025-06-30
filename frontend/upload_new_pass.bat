@echo off
cd build\web

echo Uploading with new password...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo EJR827hfffhfjsjsiior>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo index.html uploaded with simplified Flutter initialization
echo Clear your browser cache and try:
echo http://eutonafila.com.br/app/
echo.
pause 
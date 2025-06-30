@echo off
cd build\web

echo Uploading updated index.html...

echo user eutonafila> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo index.html updated with new Flutter initialization
echo Clear your browser cache completely and try:
echo http://eutonafila.com.br/app/
echo.
echo If you still see errors, please let me know the exact error message.
echo.
pause 
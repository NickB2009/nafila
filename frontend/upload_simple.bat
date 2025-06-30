@echo off
cd build\web

echo Uploading critical files...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd /www/app>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo put ..\app.htaccess .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 index.html>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Basic files uploaded. Testing access...
echo Try accessing: http://eutonafila.com.br/app/
echo.
pause 
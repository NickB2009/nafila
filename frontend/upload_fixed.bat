@echo off
cd build\web

echo Uploading with correct path handling...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo cd /www>> ftpcmd.txt
echo cd app>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo lcd .>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo put flutter.js>> ftpcmd.txt
echo put main.dart.js>> ftpcmd.txt
echo put flutter_service_worker.js>> ftpcmd.txt
echo put flutter_bootstrap.js>> ftpcmd.txt
echo put manifest.json>> ftpcmd.txt
echo put favicon.png>> ftpcmd.txt
echo put version.json>> ftpcmd.txt
echo mkdir assets>> ftpcmd.txt
echo cd assets>> ftpcmd.txt
echo lcd assets>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo lcd ..>> ftpcmd.txt
echo mkdir canvaskit>> ftpcmd.txt
echo cd canvaskit>> ftpcmd.txt
echo lcd canvaskit>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo lcd ..>> ftpcmd.txt
echo mkdir icons>> ftpcmd.txt
echo cd icons>> ftpcmd.txt
echo lcd icons>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo lcd ..>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Now uploading .htaccess file...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd /www/app>> ftpcmd.txt
echo put ..\app.htaccess .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 755 .>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 *.html>> ftpcmd.txt
echo quote SITE CHMOD 644 *.js>> ftpcmd.txt
echo quote SITE CHMOD 644 *.json>> ftpcmd.txt
echo quote SITE CHMOD 644 *.png>> ftpcmd.txt
echo quote SITE CHMOD 755 assets>> ftpcmd.txt
echo quote SITE CHMOD 755 canvaskit>> ftpcmd.txt
echo quote SITE CHMOD 755 icons>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Upload complete. Testing access...
echo Try accessing: http://eutonafila.com.br/app/
echo.
pause 
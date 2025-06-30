@echo off
cd build\web

echo Uploading JS files (final attempt)...

echo user eutonafila luqu1459> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo put flutter.js>> ftpcmd.txt
echo put main.dart.js>> ftpcmd.txt
echo put flutter_service_worker.js>> ftpcmd.txt
echo quote SITE CHMOD 644 flutter.js>> ftpcmd.txt
echo quote SITE CHMOD 644 main.dart.js>> ftpcmd.txt
echo quote SITE CHMOD 644 flutter_service_worker.js>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo JS files uploaded. Try accessing:
echo 1. http://eutonafila.com.br/app/flutter.js
echo 2. http://eutonafila.com.br/app/main.dart.js
echo.
echo Let me know if you can access these files directly
echo or if you get a 403/404 error.
echo.
pause 
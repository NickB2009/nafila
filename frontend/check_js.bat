@echo off
cd build\web

echo Checking JS files...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo get flutter.js flutter_server.js>> ftpcmd.txt
echo get main.dart.js main_server.js>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo Comparing local and server files...
echo.
fc /n flutter.js flutter_server.js
echo.
echo If the files are different or missing, we'll upload them again...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo put flutter.js>> ftpcmd.txt
echo put main.dart.js>> ftpcmd.txt
echo put flutter_service_worker.js>> ftpcmd.txt
echo quote SITE CHMOD 644 flutter.js>> ftpcmd.txt
echo quote SITE CHMOD 644 main.dart.js>> ftpcmd.txt
echo quote SITE CHMOD 644 flutter_service_worker.js>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

if exist flutter_server.js del flutter_server.js
if exist main_server.js del main_server.js

echo.
echo Files uploaded. Try accessing the app again:
echo http://eutonafila.com.br/app/
echo.
echo If you still see the syntax error, let me know the exact URL
echo that's giving you the error.
echo.
pause 
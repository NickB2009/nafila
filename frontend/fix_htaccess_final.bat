@echo off
echo Fixing .htaccess configuration (final attempt)...

echo user eutonafila> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd /www/app>> ftpcmd.txt
echo put app.htaccess .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 *.js>> ftpcmd.txt
echo quote SITE CHMOD 644 *.html>> ftpcmd.txt
echo quote SITE CHMOD 644 *.json>> ftpcmd.txt
echo quote SITE CHMOD 644 *.png>> ftpcmd.txt
echo quote SITE CHMOD 755 assets>> ftpcmd.txt
echo quote SITE CHMOD 755 canvaskit>> ftpcmd.txt
echo quote SITE CHMOD 755 icons>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo .htaccess and permissions updated
echo Try accessing these URLs to diagnose the issue:
echo 1. Main app: http://eutonafila.com.br/app/
echo 2. JS file: http://eutonafila.com.br/app/flutter.js
echo 3. HTML file: http://eutonafila.com.br/app/index.html
echo.
echo If you get different errors for different files,
echo let me know what error you see for each URL.
echo.
pause 
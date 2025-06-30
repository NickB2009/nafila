@echo off
echo Fixing file permissions in /app directory...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd app>> ftpcmd.txt
echo quote SITE CHMOD 755 .>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 *.html>> ftpcmd.txt
echo quote SITE CHMOD 644 *.js>> ftpcmd.txt
echo quote SITE CHMOD 644 *.json>> ftpcmd.txt
echo quote SITE CHMOD 644 *.png>> ftpcmd.txt
echo quote SITE CHMOD 755 assets>> ftpcmd.txt
echo cd assets>> ftpcmd.txt
echo quote SITE CHMOD 644 *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo quote SITE CHMOD 755 canvaskit>> ftpcmd.txt
echo cd canvaskit>> ftpcmd.txt
echo quote SITE CHMOD 644 *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo quote SITE CHMOD 755 icons>> ftpcmd.txt
echo cd icons>> ftpcmd.txt
echo quote SITE CHMOD 644 *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Permissions fixed. Directory listing above shows current permissions.
echo Try accessing: http://eutonafila.com.br/app/
echo.
echo If still getting 403, we'll try uploading to a different directory.
pause 
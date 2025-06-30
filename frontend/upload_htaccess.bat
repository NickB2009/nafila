@echo off
echo Uploading new .htaccess file...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo put app.htaccess .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo .htaccess file uploaded
echo Try accessing: http://eutonafila.com.br/app/
echo.
echo If still getting 403:
echo 1. Clear browser cache
echo 2. Try private/incognito window
echo 3. Check if mod_rewrite is enabled:
echo    Create a phpinfo.php file with: ^<?php phpinfo(); ?^>
echo    Upload it and access it to check Apache modules
echo.
pause 
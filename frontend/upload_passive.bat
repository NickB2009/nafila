@echo off
cd build\web

echo Uploading using passive FTP mode...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo passive>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www>> ftpcmd.txt
echo cd app>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo put app.htaccess .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 .htaccess>> ftpcmd.txt
echo quote SITE CHMOD 644 index.html>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo If the upload failed, try these troubleshooting steps:
echo 1. Check if FTP port 21 is not blocked by your firewall
echo 2. Try using an FTP client like FileZilla
echo 3. Contact KingHost support about:
echo    - FTP connection limits
echo    - IP restrictions
echo    - Firewall rules
echo.
pause 
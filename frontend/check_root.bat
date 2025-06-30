@echo off
echo Checking root directory...

echo user eutonafila> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo cd eutonafila.com.br>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo Please check the directory listing above
echo We need to find where the actual website files are located
echo This will help us understand why files uploaded to /www/app
echo are not visible on eutonafila.com.br/app
echo.
pause 
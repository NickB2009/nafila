@echo off
echo Downloading .htaccess file...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo get .htaccess>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

if exist .htaccess (
    type .htaccess
    echo.
    echo Above is the content of .htaccess
)

pause 
@echo off
echo Checking directory structure...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo cd www>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo cd public_html>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo cd htdocs>> ftpcmd.txt
echo pwd>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Directory structure checked
echo Look for:
echo 1. The actual web root directory (public_html, htdocs, www, etc)
echo 2. Any existing app or application directories
echo 3. Any existing .htaccess files
echo.
pause 
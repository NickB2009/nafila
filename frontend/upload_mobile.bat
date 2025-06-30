@echo off
cd build\web

echo Uploading mobile-optimized index.html...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo EJR827hfffhfjsjsiior>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Mobile-optimized index.html uploaded
echo On your phone:
echo 1. Clear browser cache completely
echo 2. Try accessing: http://eutonafila.com.br/app/
echo 3. If it still doesn't work, try opening in incognito/private mode
echo.
pause 
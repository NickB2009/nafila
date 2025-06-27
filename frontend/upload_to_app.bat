@echo off
cd build\web

echo Creating and uploading to app directory...

echo open ftp.eutonafila.com.br> ftpcmd.txt
echo user eutonafila>> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo mkdir app>> ftpcmd.txt
echo cd app>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo mkdir assets>> ftpcmd.txt
echo cd assets>> ftpcmd.txt
echo lcd assets>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo lcd ..>> ftpcmd.txt
echo mkdir canvaskit>> ftpcmd.txt
echo cd canvaskit>> ftpcmd.txt
echo lcd canvaskit>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo cd ..>> ftpcmd.txt
echo lcd ..>> ftpcmd.txt
echo mkdir icons>> ftpcmd.txt
echo cd icons>> ftpcmd.txt
echo lcd icons>> ftpcmd.txt
echo mput *.*>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -n -s:ftpcmd.txt
del ftpcmd.txt

echo.
echo Files uploaded to /app/ directory
echo Try accessing: http://eutonafila.com.br/app/
echo.
pause 
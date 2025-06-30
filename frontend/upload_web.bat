@echo off
cd build\web

echo Uploading rebuilt web files...

echo user eutonafila> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo binary>> ftpcmd.txt
echo cd www/app>> ftpcmd.txt
echo put index.html>> ftpcmd.txt
echo put flutter.js>> ftpcmd.txt
echo put main.dart.js>> ftpcmd.txt
echo put flutter_service_worker.js>> ftpcmd.txt
echo put flutter_bootstrap.js>> ftpcmd.txt
echo put manifest.json>> ftpcmd.txt
echo put favicon.png>> ftpcmd.txt
echo put version.json>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt

echo.
echo Files uploaded with updated base href
echo Clear your browser cache and try:
echo http://eutonafila.com.br/app/
echo.
echo If you still see the error, let me know and
echo we'll try updating the Flutter initialization code.
echo.
pause 
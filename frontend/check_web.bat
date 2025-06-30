@echo off
echo Creating test PHP file...

echo ^<?php> test.php
echo phpinfo();>> test.php

echo Uploading test file...

echo user eutonafila> ftpcmd.txt
echo luqu1459>> ftpcmd.txt
echo cd www>> ftpcmd.txt
echo put test.php>> ftpcmd.txt
echo dir>> ftpcmd.txt
echo quit>> ftpcmd.txt

ftp -s:ftpcmd.txt ftp.eutonafila.com.br
del ftpcmd.txt
del test.php

echo.
echo Test file uploaded
echo Try accessing: http://eutonafila.com.br/test.php
echo Look for:
echo 1. Server software (Apache/Nginx)
echo 2. Document root path
echo 3. Loaded modules (especially mod_rewrite)
echo.
pause 
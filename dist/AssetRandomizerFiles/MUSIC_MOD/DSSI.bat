echo off
del INPUT\fsblist.lst
for /F %%a in ('dir /b "INPUT\*.fsb"') do set fsbid=%%a
for /F "tokens=*" %%1 in ('dir /b "INPUT\*.mp3"') do echo %%1>>INPUT\fsblist.lst
listrearranger %fsbid%
fsbankcl -o OUTPUT\asd.fsb -f mp3 -q 50 INPUT\fsblist.lst
inserter %fsbid%
cd OUTPUT\
del asd.fsb
echo.
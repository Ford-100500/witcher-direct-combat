@echo off
setlocal
echo Удаление Witcher Direct Combat
echo.
set /p GAME=Вставь путь к папке The Witcher Enhanced Edition: 
if "%GAME%"=="" goto end
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1" -GameDir "%GAME%"
echo.
pause
:end

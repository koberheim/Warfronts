@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Launch-MapEditor.ps1" %*
if errorlevel 1 (
    echo.
    echo The map editor did not start. See docs\MAP_EDITOR_MANUAL.md for setup and troubleshooting.
    pause
)
endlocal

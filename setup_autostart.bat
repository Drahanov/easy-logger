@echo off
REM This script sets up EasyLogger to run automatically at Windows startup
REM Run this script as Administrator

echo ========================================
echo EasyLogger Auto-Start Setup
echo ========================================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo This will create a scheduled task to run EasyLogger at startup.
echo.
echo Please update the following path in start_logging.bat first:
echo   - Path to LibreHardwareMonitor.exe
echo.
set /p confirm="Continue? (Y/N): "
if /i not "%confirm%"=="Y" (
    echo Setup cancelled.
    pause
    exit /b 0
)

REM Get the current directory
set SCRIPT_DIR=%~dp0
set BATCH_FILE=%SCRIPT_DIR%start_logging.bat

echo.
echo Creating scheduled task...

REM Create a scheduled task that runs at startup
schtasks /create /tn "EasyLogger" /tr "\"%BATCH_FILE%\"" /sc onlogon /rl highest /f

if %errorLevel% equ 0 (
    echo.
    echo ========================================
    echo SUCCESS: Auto-start configured!
    echo ========================================
    echo.
    echo EasyLogger will now start automatically when you log in.
    echo.
    echo To disable auto-start, run:
    echo   schtasks /delete /tn "EasyLogger" /f
    echo.
) else (
    echo.
    echo ERROR: Failed to create scheduled task
    echo.
)

pause

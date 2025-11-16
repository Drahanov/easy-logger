@echo off
REM EasyLogger Startup Script
REM This script starts LibreHardwareMonitor and the temperature logger

echo Starting EasyLogger...

REM Start LibreHardwareMonitor (change path to where you installed it)
echo Starting LibreHardwareMonitor...
start "" "D:\Developer\EasyLogger\LibreHardwareMonitor-net472\LibreHardwareMonitor.exe"

REM Wait a few seconds for LibreHardwareMonitor to initialize
timeout /t 5 /nobreak >nul

REM Start the temperature logger
echo Starting Temperature Logger...
cd /d "%~dp0"
python temperature_logger.py

REM Keep window open if there's an error
if errorlevel 1 pause

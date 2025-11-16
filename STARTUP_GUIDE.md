# Auto-Start Setup Guide

This guide explains how to set up EasyLogger to run automatically when Windows starts.

## Quick Setup (Recommended)

### Step 1: Update LibreHardwareMonitor Path

1. Open `start_logging.bat` in a text editor
2. Find this line:
   ```batch
   start "" "C:\LibreHardwareMonitor\LibreHardwareMonitor.exe"
   ```
3. Update the path to where you actually installed LibreHardwareMonitor
   - Example: `"D:\Tools\LibreHardwareMonitor\LibreHardwareMonitor.exe"`

### Step 2: Run Auto-Start Setup

1. **Right-click** on `setup_autostart.bat`
2. Select **"Run as administrator"**
3. Press `Y` to confirm
4. Done! EasyLogger will now start automatically when you log in

### To Disable Auto-Start

Open Command Prompt as Administrator and run:
```batch
schtasks /delete /tn "EasyLogger" /f
```

---

## Alternative Method: Startup Folder (Simpler, but less reliable)

If you prefer a simpler method without Task Scheduler:

### Step 1: Create a Shortcut

1. Right-click on `start_logging.bat`
2. Select "Create shortcut"

### Step 2: Move to Startup Folder

1. Press `Win + R`
2. Type: `shell:startup`
3. Press Enter
4. Move the shortcut to this folder

### Step 3: Set to Run as Administrator

1. Right-click the shortcut → Properties
2. Click "Advanced..."
3. Check "Run as administrator"
4. Click OK

**Note:** Windows may still ask for UAC confirmation each time.

---

## Manual Testing

Before setting up auto-start, test the batch file manually:

1. Close any running instances of LibreHardwareMonitor and the logger
2. Double-click `start_logging.bat`
3. Verify that:
   - LibreHardwareMonitor opens
   - After 5 seconds, the temperature logger starts
   - Logs are being created in the `logs` folder

---

## Troubleshooting

### "LibreHardwareMonitor not found"
- Update the path in `start_logging.bat` to match your installation

### "Python not found"
- Make sure Python is installed and added to PATH
- Or update `start_logging.bat` to use full Python path:
  ```batch
  "C:\Users\YourName\AppData\Local\Programs\Python\Python314\python.exe" temperature_logger.py
  ```

### Logger doesn't start
- Check that `config.ini` has valid settings
- Try running `python temperature_logger.py` manually to see any errors

### UAC prompts every startup
- Use the Task Scheduler method (setup_autostart.bat) instead of Startup folder
- The scheduled task runs with highest privileges without prompting

---

## What Gets Started

When the auto-start runs, it will:

1. ✓ Start LibreHardwareMonitor as administrator
2. ✓ Wait 5 seconds for it to initialize
3. ✓ Start the Python temperature logger
4. ✓ Begin logging to the folder specified in `config.ini`

The logger will run in the background and continue logging until you:
- Restart/shutdown the computer
- Manually stop it (Ctrl+C in the window)
- Kill the process

---

## Checking if Auto-Start is Enabled

To check if the scheduled task is active:

```batch
schtasks /query /tn "EasyLogger"
```

You should see a task named "EasyLogger" with status "Ready".

---

## Managing the Logger

### View Logs
Open `log_viewer.html` in your browser and load any CSV file from the `logs` folder

### Stop Logging
Press `Ctrl+C` in the logger window

### Change Settings
Edit `config.ini` and restart the logger

### Manually Start
Double-click `start_logging.bat`

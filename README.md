# EasyLogger - Windows Temperature Logger

A simple Python-based temperature monitoring tool for Windows that logs CPU, GPU, and motherboard temperatures to help diagnose system crashes and overheating issues.

## Features

- Monitors all available temperature sensors (CPU, GPU, motherboard, etc.)
- Logs additional metrics: fan speeds, clock speeds, power consumption, load percentages
- Configurable logging interval
- CSV output format for easy analysis in Excel or other tools
- Timestamped log files
- Works with any hardware that LibreHardwareMonitor supports

## Requirements

- Windows OS
- Python 3.7 or higher
- LibreHardwareMonitor (free, open-source)

## Installation

### Step 1: Install Python

1. Download Python from [python.org](https://www.python.org/downloads/)
2. Run the installer
3. **IMPORTANT:** Check the box "Add Python to PATH" during installation
4. Complete the installation

### Step 2: Install Python Dependencies

Open Command Prompt in the project directory and run:

```bash
pip install -r requirements.txt
```

Or alternatively, install packages directly:

```bash
pip install wmi pypiwin32
```

### Step 3: Download and Run LibreHardwareMonitor

1. Download LibreHardwareMonitor from: https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/releases
2. Extract the ZIP file to a folder (e.g., `C:\LibreHardwareMonitor`)
3. **Run `LibreHardwareMonitor.exe` as Administrator** (right-click → "Run as administrator")
4. Keep LibreHardwareMonitor running in the background while using EasyLogger

> **Note:** LibreHardwareMonitor must be running for EasyLogger to work. You can minimize it to the system tray.

### Step 4: Download EasyLogger

Download or clone this repository to your computer.

## Configuration

EasyLogger can be configured using the `config.ini` file. Edit this file to set your preferred defaults:

```ini
[Settings]
# Logging interval in seconds
logging_interval = 10

# Output directory for log files
output_directory = logs
```

**Note:** Command-line arguments will override settings from `config.ini`.

## Usage

### Basic Usage

Run with settings from `config.ini`:
```bash
python temperature_logger.py
```

### Override Settings with Command-Line Arguments

Override logging interval (log every 5 seconds):
```bash
python temperature_logger.py -i 5
```

Override output directory:
```bash
python temperature_logger.py -o my_custom_logs
```

Override both settings:
```bash
python temperature_logger.py -i 15 -o crash_logs
```

**Tip:** Edit `config.ini` for your usual settings, and use command-line arguments only when you need temporary changes.

## Command-Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `-i`, `--interval` | Logging interval in seconds | From config.ini (default: 10) |
| `-o`, `--output` | Output directory for log files | From config.ini (default: logs) |

## Output Format

Log files are saved as CSV with the following format:

```csv
Timestamp,Hardware_Device,Sensor_Name,Sensor_Type,Value,Unit
2025-11-15 14:30:00,Intel Core i7-9700K,CPU Core #1,Temperature,45.5,°C
2025-11-15 14:30:00,AMD Radeon RX 6800,GPU Temperature,Temperature,62.0,°C
2025-11-15 14:30:00,Motherboard,System Fan,Fan,1200,RPM
```

The `Hardware_Device` column groups sensors by their parent hardware component (CPU, GPU, Motherboard, etc.), making it easier to identify which device each sensor belongs to.

### Log File Naming

Files are automatically named with timestamps:
- Format: `temp_log_YYYYMMDD_HHMMSS.csv`
- Example: `temp_log_20251115_143000.csv`

## Analyzing Logs

### Option 1: HTML Viewer (Recommended)

Open `log_viewer.html` in your web browser and drag-and-drop your CSV file:

**Features:**
- 📊 Beautiful interactive charts for all sensor types
- 🔍 Filter by sensor type or specific sensor
- 🖥️ Sensors grouped by hardware device (like LibreHardwareMonitor)
- 📈 Separate charts for Temperature, Fan, Load, Clock, and Power
- 📋 Statistics summary (max, min, average temperatures)
- 📱 Works completely offline (no internet required)
- 🚀 No installation needed - just open in any browser

**Usage:**
1. Double-click `log_viewer.html` to open it in your browser
2. Click "Choose CSV Log File" and select your log file
3. Use dropdown filters to view specific sensors or sensor types
4. View interactive charts and statistics grouped by hardware device

### Option 2: Traditional Tools

You can also open the CSV files in:
- **Microsoft Excel** - For custom charts and analysis
- **Google Sheets** - For online analysis and sharing
- **Python/Pandas** - For programmatic analysis
- **Any text editor** - For quick viewing

Want EasyLogger to start automatically when Windows starts? See **[STARTUP_GUIDE.md](STARTUP_GUIDE.md)** for detailed instructions.

**Quick Setup:**
1. Edit `start_logging.bat` - update the path to LibreHardwareMonitor
2. Right-click `setup_autostart.bat` → "Run as administrator"
3. Press Y to confirm
4. Done! Both LibreHardwareMonitor and the logger will start automatically on login

## License

This project is open-source. LibreHardwareMonitor is licensed under MPL 2.0.

## Contributing

Feel free to submit issues or pull requests for improvements.

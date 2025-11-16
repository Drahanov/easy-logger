"""
Temperature Logger for Windows
Monitors CPU, GPU, and motherboard temperatures using LibreHardwareMonitor
Logs data to CSV file for crash diagnosis
"""

import wmi
import csv
import time
import os
from datetime import datetime
import argparse
import configparser


class TemperatureLogger:
    def __init__(self, interval=10, output_dir="logs"):
        """
        Initialize the Temperature Logger

        Args:
            interval: Logging interval in seconds (default: 10)
            output_dir: Directory to save log files (default: "logs")
        """
        self.interval = interval
        self.output_dir = output_dir
        self.log_file = None
        self.csv_writer = None
        self.csv_file_handle = None

        # Ensure output directory exists
        if not os.path.exists(self.output_dir):
            os.makedirs(self.output_dir)

        # Initialize WMI connection
        try:
            self.w = wmi.WMI(namespace="root\\LibreHardwareMonitor")
            print("[OK] Connected to LibreHardwareMonitor")
        except Exception as e:
            print("[ERROR] Could not connect to LibreHardwareMonitor")
            print("  Make sure LibreHardwareMonitor is running with administrator privileges")
            print(f"  Technical details: {e}")
            raise

    def _create_log_file(self):
        """Create a new CSV log file with timestamp in filename"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.log_file = os.path.join(self.output_dir, f"temp_log_{timestamp}.csv")

        self.csv_file_handle = open(self.log_file, 'w', newline='')
        self.csv_writer = csv.writer(self.csv_file_handle)

        # Write header
        self.csv_writer.writerow([
            "Timestamp",
            "Hardware_Device",
            "Sensor_Name",
            "Sensor_Type",
            "Value",
            "Unit"
        ])
        self.csv_file_handle.flush()

        print(f"[OK] Created log file: {self.log_file}")

    def _extract_hardware_name(self, parent_identifier):
        """
        Extract readable hardware name from parent identifier

        Args:
            parent_identifier: String like "/amdgpu/0" or "/intelcpu/0"

        Returns:
            Readable hardware name
        """
        if not parent_identifier:
            return "Unknown"

        # Try to get the hardware object to get its name
        try:
            hardware_objects = self.w.Hardware()
            for hw in hardware_objects:
                if hw.Identifier == parent_identifier:
                    return hw.Name
        except:
            pass

        # Fallback: parse identifier string
        # Remove leading/trailing slashes and split
        parts = parent_identifier.strip('/').split('/')
        if len(parts) > 0:
            hw_type = parts[0]
            # Convert common hardware types to readable names
            type_map = {
                'amdgpu': 'AMD GPU',
                'nvidiagpu': 'NVIDIA GPU',
                'intelcpu': 'Intel CPU',
                'amdcpu': 'AMD CPU',
                'mainboard': 'Motherboard',
                'ram': 'Memory',
                'hdd': 'Hard Drive',
                'nvme': 'NVMe SSD'
            }
            return type_map.get(hw_type.lower(), hw_type.upper())

        return "Unknown"

    def get_sensor_data(self):
        """
        Read all temperature sensors from LibreHardwareMonitor

        Returns:
            List of sensor readings as dictionaries
        """
        sensors = []

        try:
            # Query all hardware sensors
            sensor_data = self.w.Sensor()

            for sensor in sensor_data:
                # We're interested in Temperature, Load, Clock, and Fan sensors
                # Filter for relevant sensor types
                if sensor.SensorType in ['Temperature', 'Load', 'Clock', 'Fan', 'Power']:
                    sensors.append({
                        'name': sensor.Name,
                        'type': sensor.SensorType,
                        'value': sensor.Value,
                        'identifier': sensor.Identifier,
                        'parent': sensor.Parent
                    })

        except Exception as e:
            print(f"Warning: Error reading sensors: {e}")

        return sensors

    def log_sensors(self):
        """Read sensors and write to CSV file"""
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        sensors = self.get_sensor_data()

        if not sensors:
            print(f"[{timestamp}] WARNING: No sensor data available!")
            return

        # Write each sensor reading to CSV
        for sensor in sensors:
            # Determine unit based on sensor type
            unit = ""
            if sensor['type'] == 'Temperature':
                unit = "°C"
            elif sensor['type'] == 'Load':
                unit = "%"
            elif sensor['type'] == 'Clock':
                unit = "MHz"
            elif sensor['type'] == 'Fan':
                unit = "RPM"
            elif sensor['type'] == 'Power':
                unit = "W"

            # Extract hardware device name from parent identifier
            # Parent format is like: "/amdgpu/0" or "/intelcpu/0"
            hardware_device = self._extract_hardware_name(sensor['parent'])

            self.csv_writer.writerow([
                timestamp,
                hardware_device,
                sensor['name'],
                sensor['type'],
                sensor['value'],
                unit
            ])

        self.csv_file_handle.flush()

        # Print summary to console
        temp_sensors = [s for s in sensors if s['type'] == 'Temperature']
        if temp_sensors:
            temp_summary = ", ".join([f"{s['name']}: {s['value']:.1f}°C" for s in temp_sensors[:5]])
            print(f"[{timestamp}] Logged {len(sensors)} sensors ({len(temp_sensors)} temperatures) - {temp_summary}")
        else:
            print(f"[{timestamp}] Logged {len(sensors)} sensors")

    def run(self):
        """Main logging loop"""
        print("\n" + "="*70)
        print("Temperature Logger Started")
        print("="*70)
        print(f"Logging interval: {self.interval} seconds")
        print(f"Output directory: {os.path.abspath(self.output_dir)}")
        print("\nPress Ctrl+C to stop logging\n")

        self._create_log_file()

        try:
            while True:
                self.log_sensors()
                time.sleep(self.interval)

        except KeyboardInterrupt:
            print("\n\n" + "="*70)
            print("Logging stopped by user")
            print("="*70)
            print(f"Log file saved: {os.path.abspath(self.log_file)}")
            print()

        finally:
            if self.csv_file_handle:
                self.csv_file_handle.close()


def load_config(config_file='config.ini'):
    """
    Load configuration from config.ini file

    Returns:
        dict: Configuration values with 'interval' and 'output_dir'
    """
    config = configparser.ConfigParser()
    defaults = {
        'interval': 10,
        'output_dir': 'logs'
    }

    if os.path.exists(config_file):
        try:
            config.read(config_file)
            if 'Settings' in config:
                interval = config.getint('Settings', 'logging_interval', fallback=defaults['interval'])
                output_dir = config.get('Settings', 'output_directory', fallback=defaults['output_dir'])
                return {'interval': interval, 'output_dir': output_dir}
        except Exception as e:
            print(f"Warning: Could not read config file '{config_file}': {e}")
            print("Using default settings")

    return defaults


def main():
    # Load settings from config file
    config = load_config()

    parser = argparse.ArgumentParser(
        description='Temperature Logger for Windows using LibreHardwareMonitor',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python temperature_logger.py                    # Use settings from config.ini
  python temperature_logger.py -i 5               # Override interval to 5 seconds
  python temperature_logger.py -i 30 -o my_logs   # Override both interval and output directory

Configuration:
  Settings can be configured in config.ini file or overridden with command-line arguments.
  Command-line arguments take precedence over config.ini settings.
        """
    )

    parser.add_argument(
        '-i', '--interval',
        type=int,
        default=None,
        help=f'Logging interval in seconds (default from config.ini: {config["interval"]})'
    )

    parser.add_argument(
        '-o', '--output',
        type=str,
        default=None,
        help=f'Output directory for log files (default from config.ini: {config["output_dir"]})'
    )

    args = parser.parse_args()

    # Use command-line arguments if provided, otherwise use config file values
    interval = args.interval if args.interval is not None else config['interval']
    output_dir = args.output if args.output is not None else config['output_dir']

    # Validate interval
    if interval < 1:
        print("Error: Interval must be at least 1 second")
        return

    try:
        logger = TemperatureLogger(interval=interval, output_dir=output_dir)
        logger.run()
    except Exception as e:
        print(f"\nFatal error: {e}")
        return 1


if __name__ == "__main__":
    main()

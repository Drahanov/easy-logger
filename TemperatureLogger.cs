using System.Globalization;
using System.Text;
using LibreHardwareMonitor.Hardware;

namespace EasyLogger;

public class TemperatureLogger : IDisposable
{
    private readonly int _interval;
    private readonly string _outputDir;
    private string? _logFilePath;
    private StreamWriter? _csvWriter;
    private Computer? _computer;
    private bool _disposed;

    public int LoggingInterval => _interval;
    public DateTime? LastReadingTime { get; private set; }

    public TemperatureLogger(int interval, string outputDir)
    {
        _interval = interval;
        _outputDir = outputDir;

        // Create output directory if it doesn't exist
        if (!Directory.Exists(_outputDir))
        {
            Directory.CreateDirectory(_outputDir);
        }
    }

    public void Initialize()
    {
        Console.WriteLine("[INFO] Initializing hardware monitoring...");

        try
        {
            // Initialize LibreHardwareMonitor Computer
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = false,
                IsStorageEnabled = true,
                IsControllerEnabled = true,
                IsPsuEnabled = true,
                IsBatteryEnabled = true
            };

            _computer.Open();
            Console.WriteLine("[OK] Hardware monitoring initialized");

            // Update hardware to get initial readings
            UpdateAllHardware();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to initialize hardware monitoring");
            Console.WriteLine($"  Make sure the application is running with administrator privileges");
            Console.WriteLine($"  Technical details: {ex.Message}");
            throw;
        }
    }

    private void UpdateAllHardware()
    {
        if (_computer == null) return;

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            // Update sub-hardware (e.g., individual CPU cores)
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Update();
            }
        }
    }

    private void CreateLogFile()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logFilePath = Path.Combine(_outputDir, $"temp_log_{timestamp}.csv");

        _csvWriter = new StreamWriter(_logFilePath, false, Encoding.UTF8);

        // Write CSV header
        _csvWriter.WriteLine("Timestamp,Hardware_Device,Sensor_Name,Sensor_Type,Value,Unit");
        _csvWriter.Flush();

        Console.WriteLine($"[OK] Created log file: {_logFilePath}");
    }

    private List<SensorReading> GetSensorData()
    {
        var readings = new List<SensorReading>();

        if (_computer == null) return readings;

        // Update all hardware before reading
        UpdateAllHardware();

        foreach (var hardware in _computer.Hardware)
        {
            CollectSensors(hardware, hardware.Name, readings);

            // Also collect from sub-hardware
            foreach (var subHardware in hardware.SubHardware)
            {
                CollectSensors(subHardware, hardware.Name, readings);
            }
        }

        return readings;
    }

    private void CollectSensors(IHardware hardware, string deviceName, List<SensorReading> readings)
    {
        foreach (var sensor in hardware.Sensors)
        {
            // Filter for relevant sensor types
            if (sensor.SensorType is SensorType.Temperature or
                                      SensorType.Load or
                                      SensorType.Clock or
                                      SensorType.Fan or
                                      SensorType.Power)
            {
                if (sensor.Value.HasValue)
                {
                    readings.Add(new SensorReading
                    {
                        DeviceName = deviceName,
                        SensorName = sensor.Name,
                        SensorType = sensor.SensorType.ToString(),
                        Value = sensor.Value.Value,
                        Unit = GetUnit(sensor.SensorType)
                    });
                }
            }
        }
    }

    private string GetUnit(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.Temperature => "°C",
            SensorType.Load => "%",
            SensorType.Clock => "MHz",
            SensorType.Fan => "RPM",
            SensorType.Power => "W",
            _ => ""
        };
    }

    private void LogSensors()
    {
        if (_csvWriter == null) return;

        LastReadingTime = DateTime.Now;
        var timestamp = LastReadingTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
        var sensors = GetSensorData();

        if (sensors.Count == 0)
        {
            Console.WriteLine($"[{timestamp}] WARNING: No sensor data available!");
            return;
        }

        // Write each sensor reading to CSV
        foreach (var sensor in sensors)
        {
            _csvWriter.WriteLine($"{timestamp},{EscapeCsv(sensor.DeviceName)},{EscapeCsv(sensor.SensorName)},{sensor.SensorType},{sensor.Value.ToString("F1", CultureInfo.InvariantCulture)},{sensor.Unit}");
        }

        _csvWriter.Flush();

        // Print summary to console
        var tempSensors = sensors.Where(s => s.SensorType == "Temperature").Take(5).ToList();
        if (tempSensors.Any())
        {
            var tempSummary = string.Join(", ", tempSensors.Select(s => $"{s.SensorName}: {s.Value:F1}°C"));
            Console.WriteLine($"[{timestamp}] Logged {sensors.Count} sensors ({sensors.Count(s => s.SensorType == "Temperature")} temperatures) - {tempSummary}");
        }
        else
        {
            Console.WriteLine($"[{timestamp}] Logged {sensors.Count} sensors");
        }
    }

    private string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public void Run()
    {
        Console.WriteLine();
        Console.WriteLine("======================================================================");
        Console.WriteLine("Temperature Logger Started");
        Console.WriteLine("======================================================================");
        Console.WriteLine($"Logging interval: {_interval} seconds");
        Console.WriteLine($"Output directory: {Path.GetFullPath(_outputDir)}");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop logging");
        Console.WriteLine();

        CreateLogFile();

        // Set up Ctrl+C handler
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("======================================================================");
            Console.WriteLine("Logging stopped by user");
            Console.WriteLine("======================================================================");
            Console.WriteLine($"Log file saved: {Path.GetFullPath(_logFilePath ?? "")}");
            Console.WriteLine();
            Environment.Exit(0);
        };

        try
        {
            while (true)
            {
                LogSensors();
                Thread.Sleep(_interval * 1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _csvWriter?.Dispose();
        _computer?.Close();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private class SensorReading
    {
        public string DeviceName { get; set; } = "";
        public string SensorName { get; set; } = "";
        public string SensorType { get; set; } = "";
        public float Value { get; set; }
        public string Unit { get; set; } = "";
    }
}

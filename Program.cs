using Microsoft.Extensions.Configuration;
using EasyLogger;

// Enable Windows Forms visual styles
System.Windows.Forms.Application.EnableVisualStyles();
System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

// Check for admin rights
if (!IsRunningAsAdministrator())
{
    System.Windows.Forms.MessageBox.Show(
        "EasyLogger needs administrator privileges to access hardware sensors.\n\n" +
        "Please:\n" +
        "  1. Close this window\n" +
        "  2. Right-click EasyLogger.exe\n" +
        "  3. Select 'Run as administrator'",
        "Administrator Privileges Required",
        System.Windows.Forms.MessageBoxButtons.OK,
        System.Windows.Forms.MessageBoxIcon.Error
    );
    return 1;
}

// Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .Build();

// Get default settings from config
var defaultInterval = config.GetValue<int>("Settings:LoggingInterval", 10);
var defaultOutputDir = config.GetValue<string>("Settings:OutputDirectory", "logs") ?? "logs";
var startMinimized = config.GetValue<bool>("Settings:StartMinimizedToTray", true);
var showNotifications = config.GetValue<bool>("Settings:ShowBalloonNotifications", true);

// Resolve output directory relative to executable location
// If it's a relative path, make it relative to the exe directory
if (!Path.IsPathRooted(defaultOutputDir))
{
    defaultOutputDir = Path.Combine(AppContext.BaseDirectory, defaultOutputDir);
}

// Parse command-line arguments
var interval = defaultInterval;
var outputDir = defaultOutputDir;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-i" or "--interval":
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedInterval))
            {
                interval = parsedInterval;
                i++;
            }
            else
            {
                Console.WriteLine("Error: Invalid interval value");
                PrintUsage();
                return 1;
            }
            break;

        case "-o" or "--output":
            if (i + 1 < args.Length)
            {
                outputDir = args[i + 1];
                i++;
            }
            else
            {
                Console.WriteLine("Error: Missing output directory value");
                PrintUsage();
                return 1;
            }
            break;

        case "-h" or "--help":
            PrintUsage();
            return 0;

        default:
            Console.WriteLine($"Error: Unknown argument '{args[i]}'");
            PrintUsage();
            return 1;
    }
}

// Validate interval
if (interval < 1)
{
    interval = 1;
}

// Check if auto-start is enabled
bool autoStartEnabled = IsAutoStartEnabled();

// Initialize system tray icon
TrayIconManager? trayIcon = null;
TemperatureLogger? logger = null;
SettingsForm? settingsForm = null;

try
{
    logger = new TemperatureLogger(interval, outputDir);
    logger.Initialize();

    // Create settings form
    settingsForm = new SettingsForm(logger, outputDir, autoStartEnabled);

    // Initialize tray icon with settings form
    trayIcon = new TrayIconManager(outputDir, settingsForm, startMinimized);

    // Run logger on background thread
    var loggerThread = new Thread(() =>
    {
        try
        {
            logger.Run();
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                $"Fatal error in logging thread: {ex.Message}",
                "EasyLogger Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error
            );
            System.Windows.Forms.Application.Exit();
        }
    })
    {
        IsBackground = true,
        Name = "Logger Thread"
    };

    loggerThread.Start();

    // Run Windows Forms message loop on main thread
    System.Windows.Forms.Application.Run();

    return 0;
}
catch (Exception ex)
{
    System.Windows.Forms.MessageBox.Show(
        $"Fatal error: {ex.Message}\n\nPlease make sure you're running as administrator.",
        "EasyLogger Error",
        System.Windows.Forms.MessageBoxButtons.OK,
        System.Windows.Forms.MessageBoxIcon.Error
    );
    return 1;
}
finally
{
    logger?.Dispose();
    trayIcon?.Dispose();
    settingsForm?.Dispose();
}

static bool IsRunningAsAdministrator()
{
    try
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
    catch
    {
        return false;
    }
}

static bool IsAutoStartEnabled()
{
    try
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = "/Query /TN \"EasyLogger\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }
    catch
    {
        return false;
    }
}

static void PrintUsage()
{
    System.Windows.Forms.MessageBox.Show(
        "EasyLogger - Temperature Monitoring for Windows\n\n" +
        "Usage:\n" +
        "  EasyLogger.exe [options]\n\n" +
        "Options:\n" +
        "  -i, --interval <seconds>    Logging interval in seconds (default: 10)\n" +
        "  -o, --output <directory>    Output directory for log files (default: logs)\n" +
        "  -h, --help                  Show this help message\n\n" +
        "Configuration:\n" +
        "  Settings can be configured via the system tray icon.",
        "EasyLogger Help",
        System.Windows.Forms.MessageBoxButtons.OK,
        System.Windows.Forms.MessageBoxIcon.Information
    );
}

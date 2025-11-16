using System.Diagnostics;
using System.Windows.Forms;

namespace EasyLogger;

public class SettingsForm : Form
{
    private readonly TemperatureLogger _logger;
    private readonly string _logsDirectory;
    private System.Windows.Forms.Timer _statusUpdateTimer;

    // UI Controls
    private Label _statusLabel;
    private Label _lastReadingLabel;
    private CheckBox _autoStartCheckbox;
    private NumericUpDown _intervalInput;
    private TextBox _logDirectoryInput;
    private Button _browseButton;
    private Button _openViewerButton;
    private Button _openLogsButton;
    private Button _saveButton;
    private Button _closeButton;
    private GroupBox _settingsGroup;

    public SettingsForm(TemperatureLogger logger, string logsDirectory, bool autoStartEnabled)
    {
        _logger = logger;
        _logsDirectory = logsDirectory;

        InitializeComponent();
        LoadSettings(autoStartEnabled);
        SetupStatusTimer();
    }

    private void InitializeComponent()
    {
        // Form settings
        this.Text = "EasyLogger Settings";
        this.Size = new System.Drawing.Size(500, 430);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Load custom icon
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new System.Drawing.Icon(iconPath);
            }
            else
            {
                this.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            this.Icon = System.Drawing.SystemIcons.Application;
        }

        // Prevent closing, just hide
        this.FormClosing += (s, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        };

        int yPos = 20;

        // Status Label
        _statusLabel = new Label
        {
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 25),
            Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
            Text = "● Logging Active"
        };
        this.Controls.Add(_statusLabel);
        yPos += 30;

        // Last reading label
        _lastReadingLabel = new Label
        {
            Location = new System.Drawing.Point(40, yPos),
            Size = new System.Drawing.Size(450, 20),
            Font = new System.Drawing.Font("Segoe UI", 9),
            ForeColor = System.Drawing.Color.Gray,
            Text = "Last reading: Just now"
        };
        this.Controls.Add(_lastReadingLabel);
        yPos += 35;

        // Settings GroupBox
        _settingsGroup = new GroupBox
        {
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(450, 180),
            Text = "Settings"
        };
        this.Controls.Add(_settingsGroup);

        int groupYPos = 25;

        // Auto-start checkbox
        _autoStartCheckbox = new CheckBox
        {
            Location = new System.Drawing.Point(15, groupYPos),
            Size = new System.Drawing.Size(400, 25),
            Text = "Start automatically with Windows",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _settingsGroup.Controls.Add(_autoStartCheckbox);
        groupYPos += 35;

        // Logging interval label
        var intervalLabel = new Label
        {
            Location = new System.Drawing.Point(15, groupYPos),
            Size = new System.Drawing.Size(150, 20),
            Text = "Logging Interval:",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _settingsGroup.Controls.Add(intervalLabel);

        _intervalInput = new NumericUpDown
        {
            Location = new System.Drawing.Point(160, groupYPos - 2),
            Size = new System.Drawing.Size(80, 25),
            Minimum = 1,
            Maximum = 3600,
            Value = 10
        };
        _settingsGroup.Controls.Add(_intervalInput);

        var secondsLabel = new Label
        {
            Location = new System.Drawing.Point(245, groupYPos),
            Size = new System.Drawing.Size(100, 20),
            Text = "seconds",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _settingsGroup.Controls.Add(secondsLabel);
        groupYPos += 35;

        // Log directory label
        var logDirLabel = new Label
        {
            Location = new System.Drawing.Point(15, groupYPos),
            Size = new System.Drawing.Size(150, 20),
            Text = "Log Directory:",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _settingsGroup.Controls.Add(logDirLabel);
        groupYPos += 25;

        // Log directory input
        _logDirectoryInput = new TextBox
        {
            Location = new System.Drawing.Point(15, groupYPos),
            Size = new System.Drawing.Size(340, 25),
            ReadOnly = true,
            BackColor = System.Drawing.SystemColors.Window
        };
        _settingsGroup.Controls.Add(_logDirectoryInput);

        // Browse button
        _browseButton = new Button
        {
            Location = new System.Drawing.Point(360, groupYPos - 2),
            Size = new System.Drawing.Size(70, 25),
            Text = "Browse...",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _browseButton.Click += BrowseButton_Click;
        _settingsGroup.Controls.Add(_browseButton);

        yPos += 200;

        // Action buttons row
        _openViewerButton = new Button
        {
            Location = new System.Drawing.Point(20, yPos),
            Size = new System.Drawing.Size(130, 35),
            Text = "Open Log Viewer",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _openViewerButton.Click += OpenViewer_Click;
        this.Controls.Add(_openViewerButton);

        _openLogsButton = new Button
        {
            Location = new System.Drawing.Point(160, yPos),
            Size = new System.Drawing.Size(110, 35),
            Text = "Open Logs",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _openLogsButton.Click += OpenLogs_Click;
        this.Controls.Add(_openLogsButton);

        yPos += 50;

        // Save and Close buttons
        _saveButton = new Button
        {
            Location = new System.Drawing.Point(270, yPos),
            Size = new System.Drawing.Size(100, 35),
            Text = "Save Settings",
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
            BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _saveButton.Click += SaveButton_Click;
        this.Controls.Add(_saveButton);

        _closeButton = new Button
        {
            Location = new System.Drawing.Point(380, yPos),
            Size = new System.Drawing.Size(90, 35),
            Text = "Close",
            Font = new System.Drawing.Font("Segoe UI", 9)
        };
        _closeButton.Click += (s, e) => this.Hide();
        this.Controls.Add(_closeButton);
    }

    private void LoadSettings(bool autoStartEnabled)
    {
        _autoStartCheckbox.Checked = autoStartEnabled;
        _intervalInput.Value = _logger.LoggingInterval;
        _logDirectoryInput.Text = _logsDirectory;
    }

    private void SetupStatusTimer()
    {
        _statusUpdateTimer = new System.Windows.Forms.Timer();
        _statusUpdateTimer.Interval = 1000; // Update every second
        _statusUpdateTimer.Tick += UpdateStatus;
        _statusUpdateTimer.Start();
    }

    private void UpdateStatus(object? sender, EventArgs e)
    {
        var lastReading = _logger.LastReadingTime;
        if (lastReading.HasValue)
        {
            var elapsed = DateTime.Now - lastReading.Value;
            _statusLabel.Text = "● Logging Active";
            _statusLabel.ForeColor = System.Drawing.Color.Green;

            if (elapsed.TotalSeconds < 2)
            {
                _lastReadingLabel.Text = "Last reading: Just now";
            }
            else if (elapsed.TotalSeconds < 60)
            {
                _lastReadingLabel.Text = $"Last reading: {(int)elapsed.TotalSeconds} seconds ago";
            }
            else
            {
                _lastReadingLabel.Text = $"Last reading: {(int)elapsed.TotalMinutes} minutes ago";
            }
        }
        else
        {
            _statusLabel.Text = "○ Starting...";
            _statusLabel.ForeColor = System.Drawing.Color.Orange;
            _lastReadingLabel.Text = "Waiting for first reading...";
        }
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder for temperature logs",
            SelectedPath = _logDirectoryInput.Text,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _logDirectoryInput.Text = dialog.SelectedPath;
        }
    }

    private void OpenViewer_Click(object? sender, EventArgs e)
    {
        try
        {
            var viewerPath = Path.Combine(AppContext.BaseDirectory, "log_viewer.html");
            if (File.Exists(viewerPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = viewerPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show(
                    "log_viewer.html not found in the installation directory.",
                    "File Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open log viewer: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void OpenLogs_Click(object? sender, EventArgs e)
    {
        try
        {
            if (Directory.Exists(_logsDirectory))
            {
                Process.Start("explorer.exe", _logsDirectory);
            }
            else
            {
                MessageBox.Show(
                    $"Logs directory does not exist yet:\n{_logsDirectory}\n\nLogs will be created after the first reading.",
                    "Directory Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open logs folder: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            var newInterval = (int)_intervalInput.Value;
            var newDirectory = _logDirectoryInput.Text;
            var autoStart = _autoStartCheckbox.Checked;

            // Update appsettings.json
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var settings = new
            {
                Settings = new
                {
                    LoggingInterval = newInterval,
                    OutputDirectory = newDirectory,
                    StartMinimizedToTray = true,
                    ShowBalloonNotifications = true
                }
            };

            File.WriteAllText(settingsPath, System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));

            // Update auto-start task
            UpdateAutoStartTask(autoStart);

            MessageBox.Show(
                "Settings saved successfully!\n\nNote: Logging interval and directory changes will take effect after restarting the application.",
                "Settings Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save settings: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void UpdateAutoStartTask(bool enable)
    {
        try
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";

            if (enable)
            {
                // Create/update scheduled task
                var createCmd = $"schtasks /Create /TN \"EasyLogger\" /TR \"\\\"{exePath}\\\"\" /SC ONLOGON /RL HIGHEST /F";
                ExecuteCommand(createCmd);
            }
            else
            {
                // Delete scheduled task
                var deleteCmd = "schtasks /Delete /TN \"EasyLogger\" /F";
                ExecuteCommand(deleteCmd);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to update auto-start setting: {ex.Message}\n\nYou may need to run as administrator.",
                "Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    private void ExecuteCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        process.WaitForExit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusUpdateTimer?.Stop();
            _statusUpdateTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}

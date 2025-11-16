using System.Diagnostics;

namespace EasyLogger;

public class TrayIconManager : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.ContextMenuStrip _contextMenu;
    private readonly string _logsDirectory;
    private readonly SettingsForm _settingsForm;
    private bool _disposed;

    public TrayIconManager(string logsDirectory, SettingsForm settingsForm, bool startMinimized = true)
    {
        _logsDirectory = logsDirectory;
        _settingsForm = settingsForm;

        // Create context menu
        _contextMenu = new System.Windows.Forms.ContextMenuStrip();

        var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Settings...");
        settingsItem.Click += (s, e) => ShowSettings();
        _contextMenu.Items.Add(settingsItem);

        _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var openLogsItem = new System.Windows.Forms.ToolStripMenuItem("Open Logs Folder");
        openLogsItem.Click += (s, e) => OpenLogsFolder();
        _contextMenu.Items.Add(openLogsItem);

        var openViewerItem = new System.Windows.Forms.ToolStripMenuItem("Open Log Viewer");
        openViewerItem.Click += (s, e) => OpenLogViewer();
        _contextMenu.Items.Add(openViewerItem);

        _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Quit EasyLogger");
        exitItem.Click += (s, e) => ExitApplication();
        _contextMenu.Items.Add(exitItem);

        // Create tray icon
        _trayIcon = new System.Windows.Forms.NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Text = "EasyLogger - Temperature Monitor",
            Visible = true
        };

        // Load custom icon
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
            {
                _trayIcon.Icon = new System.Drawing.Icon(iconPath);
            }
            else
            {
                _trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        // Left-click to show settings, right-click for menu
        _trayIcon.Click += (s, e) =>
        {
            var mouseEvent = e as System.Windows.Forms.MouseEventArgs;
            if (mouseEvent != null && mouseEvent.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowSettings();
            }
        };

        // Show startup notification
        _trayIcon.ShowBalloonTip(
            2000,
            "EasyLogger Started",
            "Temperature logging is active. Click icon for settings.",
            System.Windows.Forms.ToolTipIcon.Info
        );
    }

    private void ShowSettings()
    {
        _settingsForm.Show();
        _settingsForm.BringToFront();
        _settingsForm.Activate();
    }

    private void OpenLogsFolder()
    {
        try
        {
            if (Directory.Exists(_logsDirectory))
            {
                Process.Start("explorer.exe", _logsDirectory);
            }
            else
            {
                _trayIcon.ShowBalloonTip(
                    3000,
                    "Logs Folder Not Found",
                    $"The logs directory does not exist yet: {_logsDirectory}",
                    System.Windows.Forms.ToolTipIcon.Warning
                );
            }
        }
        catch (Exception ex)
        {
            _trayIcon.ShowBalloonTip(
                3000,
                "Error",
                $"Failed to open logs folder: {ex.Message}",
                System.Windows.Forms.ToolTipIcon.Error
            );
        }
    }

    private void OpenLogViewer()
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
                _trayIcon.ShowBalloonTip(
                    3000,
                    "Log Viewer Not Found",
                    "log_viewer.html is missing from the installation directory.",
                    System.Windows.Forms.ToolTipIcon.Warning
                );
            }
        }
        catch (Exception ex)
        {
            _trayIcon.ShowBalloonTip(
                3000,
                "Error",
                $"Failed to open log viewer: {ex.Message}",
                System.Windows.Forms.ToolTipIcon.Error
            );
        }
    }

    private void ExitApplication()
    {
        var result = System.Windows.Forms.MessageBox.Show(
            "Are you sure you want to stop temperature logging and exit?",
            "Quit EasyLogger",
            System.Windows.Forms.MessageBoxButtons.YesNo,
            System.Windows.Forms.MessageBoxIcon.Question
        );

        if (result == System.Windows.Forms.DialogResult.Yes)
        {
            System.Windows.Forms.Application.Exit();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _trayIcon?.Dispose();
        _contextMenu?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

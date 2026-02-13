using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TEdit.Configuration;
using TEdit.ViewModel;

namespace TEdit;

public static class ErrorLogging
{
    private static TelemetryClient _telemetry;
    private const string UserPathRegex = @"C:\\Users\\([^\\]*)\\";
    private const string TelemetryConnectionString = "InstrumentationKey=8c8ff827-554e-4838-a6b6-e3d837519e51;IngestionEndpoint=https://dc.services.visualstudio.com";

    public static void ViewLog()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = LogFilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to open log file: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    public static void Initialize()
    {
        lock (LogFilePath)
        {
            string dir = Path.GetDirectoryName(LogFilePath);
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Unable to create log folder. Application will exit.\r\n{dir}\r\n{ex.Message}",
                "Unable to create undo folder.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                App.Current.Shutdown();
            }

            // Roll the log file if it exceeds 100MB
            RollLogIfNeeded();

            // Clean up old rolled log files (keep last 5)
            CleanupOldLogs(dir);
        }

        InitializeTelemetry();
    }

    private const long MaxLogFileSize = 100 * 1024 * 1024; // 100MB

    private static void RollLogIfNeeded()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                var fileInfo = new FileInfo(LogFilePath);
                if (fileInfo.Length >= MaxLogFileSize)
                {
                    // Roll the log files (TEdit.txt -> TEdit.1.txt -> TEdit.2.txt -> etc.)
                    string logDirectory = Path.GetDirectoryName(LogFilePath);
                    string baseFileName = Path.GetFileNameWithoutExtension(LogFilePath); // "TEdit"

                    // Delete the oldest log (TEdit.4.txt) if it exists
                    string oldestLog = Path.Combine(logDirectory, $"{baseFileName}.4.txt");
                    if (File.Exists(oldestLog))
                        File.Delete(oldestLog);

                    // Roll TEdit.3.txt -> TEdit.4.txt, TEdit.2.txt -> TEdit.3.txt, TEdit.1.txt -> TEdit.2.txt
                    for (int i = 3; i >= 1; i--)
                    {
                        string oldFile = Path.Combine(logDirectory, $"{baseFileName}.{i}.txt");
                        string newFile = Path.Combine(logDirectory, $"{baseFileName}.{i + 1}.txt");
                        if (File.Exists(oldFile))
                            File.Move(oldFile, newFile);
                    }

                    // Move current log to TEdit.1.txt
                    string firstBackup = Path.Combine(logDirectory, $"{baseFileName}.1.txt");
                    File.Move(LogFilePath, firstBackup);

                    Log($"Log file rolled. Previous log saved as {baseFileName}.1.txt");
                }
            }
        }
        catch (Exception ex)
        {
            // If rolling fails, just continue - we'll append to the large file
            try { Log($"Warning: Failed to roll log file: {ex.Message}"); } catch { }
        }
    }

    private static void CleanupOldLogs(string logDirectory)
    {
        try
        {
            // Clean up old timestamped log files from previous versions
            var oldTimestampedLogs = Directory.GetFiles(logDirectory, "TEditLog_*.txt");
            foreach (string file in oldTimestampedLogs)
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-7))
                    {
                        fi.Delete();
                    }
                }
                catch
                {
                    // Ignore errors deleting old log files
                }
            }
        }
        catch
        {
            // Ignore errors in cleanup
        }
    }

    public static void InitializeTelemetry()
    {
        if (UserSettingsService.Current.Telemetry != 0)
        {
            try
            {
                _telemetry = GetAppInsightsClient();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        else
        {
            _telemetry = null;
        }
    }

    private static TelemetryClient GetAppInsightsClient()
    {
        var config = new TelemetryConfiguration();
        config.ConnectionString = TelemetryConnectionString;
        config.TelemetryChannel = new Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.ServerTelemetryChannel();
        config.TelemetryChannel.DeveloperMode = Debugger.IsAttached;
#if DEBUG
        config.TelemetryChannel.DeveloperMode = true;
#endif
        TelemetryClient client = new TelemetryClient(config);
        client.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
        client.Context.Session.Id = Guid.NewGuid().ToString();
        client.Context.User.Id = "TEdit";
        client.Context.Cloud.RoleInstance = "TEdit-Wpf";
        client.Context.GlobalProperties["Version"] = App.Version.ToString();
        // client.Context.Device.Model
        client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
        return client;
    }

    public static string LogFilePath = Path.Combine(WorldViewModel.TempPath, "Logs", "TEdit.txt");

    #region ErrorLevel enum

    public enum ErrorLevel
    {
        Debug,
        Trace,
        Warn,
        Error,
        Fatal
    }

    #endregion

    public static void Log(string message)
    {
        message = Regex.Replace(message, UserPathRegex, "C:\\Users\\[user]\\");

        lock (LogFilePath)
        {
            File.AppendAllText(LogFilePath,
                $"{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}: {message} {Environment.NewLine}");
        }
    }

    public static void LogException(Exception ex)
    {
        if (ex is AggregateException)
        {
            var e = ex as AggregateException;
            foreach (var curE in e.Flatten().InnerExceptions) LogException(curE);
        }
        else
        {
            // Log inner exceptions first
            if (ex.InnerException != null)
                LogException(ex.InnerException);

            Log($"{ErrorLevel.Error} - {ex.Message}\r\n{ex.StackTrace}");

            if (_telemetry != null && UserSettingsService.Current.Telemetry == 1)
            {
                var telex = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                telex.Message = Regex.Replace(ex.Message ?? string.Empty, UserPathRegex, "C:\\Users\\[user]\\");
                _telemetry.TrackException(telex);
                _telemetry.Flush();
            }
        }
    }

    public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
}

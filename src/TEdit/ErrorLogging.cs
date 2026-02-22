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

                    LogInfo($"Log file rolled. Previous log saved as {baseFileName}.1.txt");
                }
            }
        }
        catch (Exception ex)
        {
            // If rolling fails, just continue - we'll append to the large file
            try { LogWarn($"Failed to roll log file: {ex.Message}"); } catch { }
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
        config.DisableTelemetry = false;

        TelemetryClient client = new TelemetryClient(config);
        client.Context.User.Id = "TEdit";
        client.Context.GlobalProperties["Version"] = App.Version.ToString();
        client.Context.GlobalProperties["AppVersion"] = Assembly.GetEntryAssembly().GetName().Version.ToString();
        client.Context.GlobalProperties["SessionId"] = Guid.NewGuid().ToString();
        client.Context.GlobalProperties["RoleInstance"] = "TEdit-Wpf";
        client.Context.GlobalProperties["OperatingSystem"] = Environment.OSVersion.ToString();
        return client;
    }

    public static string LogFilePath = Path.Combine(WorldViewModel.TempPath, "Logs", "TEdit.txt");

    #region ErrorLevel enum

    public enum ErrorLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    #endregion

    private static string LevelTag(ErrorLevel level) => level switch
    {
        ErrorLevel.Trace => "[TRC]",
        ErrorLevel.Debug => "[DBG]",
        ErrorLevel.Info  => "[INF]",
        ErrorLevel.Warn  => "[WRN]",
        ErrorLevel.Error => "[ERR]",
        ErrorLevel.Fatal => "[FTL]",
        _                => "[INF]"
    };

    public static void Log(ErrorLevel level, string message)
    {
        message = Regex.Replace(message, UserPathRegex, "C:\\Users\\[user]\\");

        lock (LogFilePath)
        {
            File.AppendAllText(LogFilePath,
                $"{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")} {LevelTag(level)} {message} {Environment.NewLine}");
        }
    }

    public static void Log(string message) => Log(ErrorLevel.Info, message);

    public static void LogTrace(string message) => Log(ErrorLevel.Trace, message);
    public static void LogDebug(string message) => Log(ErrorLevel.Debug, message);
    public static void LogInfo(string message) => Log(ErrorLevel.Info, message);
    public static void LogWarn(string message) => Log(ErrorLevel.Warn, message);
    public static void LogError(string message) => Log(ErrorLevel.Error, message);
    public static void LogFatal(string message) => Log(ErrorLevel.Fatal, message);

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

            LogError($"{ex.Message}\r\n{ex.StackTrace}");

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

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
    private static StreamWriter? _logWriter;
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
        try
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"Unable to create log folder. Application will exit.\r\n{LogDirectory}\r\n{ex.Message}",
            "Unable to create log folder.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            App.Current.Shutdown();
        }

        CleanupOldLogs();
        _logWriter = OpenLogWriter(LogFilePath);

        InitializeTelemetry();
    }

    private static StreamWriter? OpenLogWriter(string basePath)
    {
        // Try the base path first, then increment suffix if locked
        string dir = Path.GetDirectoryName(basePath)!;
        string name = Path.GetFileNameWithoutExtension(basePath);
        string ext = Path.GetExtension(basePath);

        for (int i = 0; i < 10; i++)
        {
            string path = i == 0
                ? basePath
                : Path.Combine(dir, $"{name}.{i:D3}{ext}");

            try
            {
                var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                LogFilePath = path;
                return new StreamWriter(stream) { AutoFlush = true };
            }
            catch (IOException) { }
        }

        return null;
    }

    private const long MaxLogFileSize = 100 * 1024 * 1024; // 100MB

    private static void RollIfNeeded()
    {
        if (_logWriter == null) return;

        try
        {
            if (_logWriter.BaseStream.Length >= MaxLogFileSize)
            {
                _logWriter.Dispose();
                var newPath = Path.Combine(LogDirectory, $"TEdit_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                _logWriter = OpenLogWriter(newPath);
            }
        }
        catch (IOException) { }
    }

    private static void CleanupOldLogs()
    {
        try
        {
            // Clean up log files older than 7 days (both old and new naming)
            foreach (string pattern in new[] { "TEdit_*.txt", "TEditLog_*.txt", "TEdit.*.txt" })
            {
                foreach (string file in Directory.GetFiles(LogDirectory, pattern))
                {
                    try
                    {
                        if (new FileInfo(file).LastWriteTime < DateTime.Now.AddDays(-7))
                            File.Delete(file);
                    }
                    catch { }
                }
            }
        }
        catch { }
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
        client.Context.GlobalProperties["AppVersion"] = App.Version.ToString();
        client.Context.GlobalProperties["SessionId"] = Guid.NewGuid().ToString();
        client.Context.GlobalProperties["RoleInstance"] = "TEdit-Wpf";
        client.Context.GlobalProperties["OperatingSystem"] = Environment.OSVersion.ToString();
        return client;
    }

    public static readonly string LogDirectory = Path.Combine(WorldViewModel.TempPath, "Logs");
    public static string LogFilePath = Path.Combine(LogDirectory, $"TEdit_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

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

    private static readonly object _logLock = new();

    public static void Log(ErrorLevel level, string message)
    {
        message = Regex.Replace(message, UserPathRegex, "C:\\Users\\[user]\\");

        lock (_logLock)
        {
            try
            {
                RollIfNeeded();
                _logWriter?.WriteLine($"{DateTime.Now:MM-dd-yyyy HH:mm:ss} {LevelTag(level)} {message}");
            }
            catch (IOException) { }
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

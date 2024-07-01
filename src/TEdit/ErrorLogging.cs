using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TEdit.Properties;
using TEdit.ViewModel;

namespace TEdit;

public static class ErrorLogging
{
    private static TelemetryClient _telemetry;
    private const string UserPathRegex = @"C:\\Users\\([^\\]*)\\";
    private const string TelemetryKey = "8c8ff827-554e-4838-a6b6-e3d837519e51";

    public static void ViewLog()
    {
        Process.Start(LogFilePath);
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

            // cleanup old 
            foreach (string file in Directory.GetFiles(dir))
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-7))
                    {
                        fi.Delete();
                    }
                }
                catch (Exception)
                {
                    // skip deleting this file
                }
            }

            try
            {
                File.Create(LogFilePath).Dispose();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Unable to create log file. Application will exit.\r\n{dir}\r\n{ex.Message}",
                "Unable to create undo folder.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                App.Current.Shutdown();
            }
        }

        InitializeTelemetry();
    }

    public static TelemetryClient TelemetryClient => _telemetry;

    public static void InitializeTelemetry()
    {
        if (Settings.Default.Telemetry != 0)
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
        config.InstrumentationKey = TelemetryKey;
        config.TelemetryChannel = new Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.ServerTelemetryChannel();
        //config.TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(); // Default channel
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

    public static string LogFilePath = Path.Combine(WorldViewModel.TempPath, "Logs", $"TEditLog_{DateTime.Now:yyyyMMddHHmmss}.txt");

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

            if (Settings.Default.Telemetry == 1)
            {
                var telex = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                _telemetry?.TrackException(telex);
                _telemetry?.Flush();
            }
        }
    }

    public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TEditXna
{
    public static class ErrorLogging
    {
        public static void ViewLog()
        {
            Process.Start(LogFilePath);
        }

        public static void Initialize()
        {
            lock (LogFilePath)
            {
                string dir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (File.Exists(LogFilePath))
                {
                    string destFileName = LogFilePath + ".old";
                    if (File.Exists(destFileName))
                        File.Delete(destFileName);
                    File.Move(LogFilePath, destFileName);
                }
                else
                {
                    File.Create(LogFilePath).Dispose();
                   
                }
            }
        }

        public static string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Terraria", "TEditLog.txt");

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
            lock (LogFilePath)
            {
                File.AppendAllText(LogFilePath,
                    $"{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}: {message} {Environment.NewLine}");
            }
        }

        public static void LogException(object ex)
        {
            if (ex is AggregateException)
            {
                var e = ex as AggregateException;
                foreach (var curE in e.Flatten().InnerExceptions) LogException(curE);
            }
            else if (ex is Exception)
            {
                var e = ex as Exception;
                // Log inner exceptions first
                if (e.InnerException != null)
                    LogException(e.InnerException);

                Log($"{ErrorLevel.Error} - {e.Message}\r\n{e.StackTrace}");
            }
        }

        public static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return fvi.FileVersion;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TerrariaMapEditor.Common
{
    public static class ErrorLogging
    {
        public static void Log(string message)
        {
            File.AppendAllText(Properties.Settings.Default.LogFile, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message + Environment.NewLine);
        }

        public static void LogException(Exception e)
        {
            Log(String.Format("{0} - {1}\r\n{2}", ErrorLevel.Error, e.Message, e.StackTrace));
        }

        public static void LogException(Exception e, ErrorLevel level)
        {
            Log(String.Format("{0} - {1}\r\n{2}", level, e.Message, e.StackTrace));
        }

        public enum ErrorLevel
        {
            Debug,
            Trace,
            Warn,
            Error,
            Fatal
        }
    }
}

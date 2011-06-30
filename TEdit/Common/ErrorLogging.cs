using System;
using System.IO;

namespace TEdit.Common
{
    public static class ErrorLogging
    {
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
            File.AppendAllText("log.txt",
                               DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                               message + Environment.NewLine);
        }

        public static void LogException(object ex)
        {
            var e = ex as Exception;
            if (e != null)
            {
                Log(String.Format("{0} - {1}\r\n{2}", ErrorLevel.Error, e.Message, e.StackTrace));
            }
        }

        public static void LogException(object ex, ErrorLevel level)
        {
            var e = ex as Exception;
            if (e != null)
            {
                Log(String.Format("{0} - {1}\r\n{2}", level, e.Message, e.StackTrace));
            }
        }
    }
}
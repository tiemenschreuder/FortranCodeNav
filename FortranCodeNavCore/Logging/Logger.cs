using System;
using System.IO;

namespace FortranCodeNavCore.Logging
{
    public static class Log
    {
        private static readonly FileStream LogFile;
        private static readonly TextWriter LogWriter;

        static Log()
        {
            var path = Path.GetTempPath();
            LogFile = File.Open(path + "FortranCodeNav.log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            LogWriter = new StreamWriter(LogFile);       
        }

        public static void Error(string logMessage, Exception e, string context=null)
        {
            Write(string.Format("{0}: {1}{2}", logMessage, e, context ?? ""));
        }

        public static void Write(string logMessage)
        {
            if (!LogFile.CanWrite)
            {
                return;
            }

            var datetime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            if (datetime.Length < 20)
            {
                datetime = datetime.PadRight(20);
            }

            lock (LogWriter)
            {
                LogWriter.WriteLine(datetime + ": " + logMessage);
                LogWriter.Flush();
            }
        }

        public static void CloseLog()
        {
            if (LogWriter != null)
            {
                LogWriter.Close();
            }
        }
    }
}

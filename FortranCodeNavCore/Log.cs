using System;
using System.IO;

namespace FortranCodeNavCore
{
    public static class Log
    {
        private static readonly FileStream LogFile;
        private static readonly TextWriter LogWriter;

        static Log()
        {
            var path = Path.GetTempPath();
            var logFilePath = path + "FortranCodeNav.log";

            TrimSize(logFilePath);

            LogFile = File.Open(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            LogWriter = new StreamWriter(LogFile);       
        }

        private static void TrimSize(string logFilePath)
        {
            const int maxSize = 1 * 1024 * 1024; //max 1mb logfile

            var logInfo = new FileInfo(logFilePath);

            if (!logInfo.Exists || logInfo.Length <= maxSize) 
                return;

            try
            {
                using (var ms = new MemoryStream(maxSize))
                {
                    using (var s = new FileStream(logFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        s.Seek(-maxSize, SeekOrigin.End); //last 1mb
                        CopyTo(s,ms); //copy to buffer
                        s.SetLength(maxSize); //trim to 1mb
                        s.Position = 0; //begin of file
                        ms.Position = 0; //begin of stream
                        CopyTo(ms,s); //copy from buffer
                    }
                }
            }
            catch (Exception)
            {
                //gulp (we can't log here)
            }
        }

        private static void CopyTo(Stream source, Stream destination)
        {
            int num;
            var buffer = new byte[4096];
            while ((num = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, num);
            }
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

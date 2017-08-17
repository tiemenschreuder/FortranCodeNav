using System;

namespace VSIntegration
{
    public static class VSLogger
    {
        public static ILogger RealLogger { get; set; }

        public static void Error(string logMessage, Exception e, string context=null)
        {
            if (RealLogger != null)
            {
                RealLogger.Error("VS: " + logMessage, e, context);
            }
        }

        public static void Write(string logMessage)
        {
            if (RealLogger != null)
            {
                RealLogger.Write("VS: " + logMessage);
            }
        }
    }
}

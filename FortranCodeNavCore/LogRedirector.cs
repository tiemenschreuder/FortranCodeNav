using System;
using VSIntegration;

namespace FortranCodeNavCore
{
    public class LogRedirector : ILogger
    {
        public void Error(string logMessage, Exception e, string context = null)
        {
            Log.Error(logMessage, e, context);
        }

        public void Write(string logMessage)
        {
            Log.Write(logMessage);
        }
    }
}
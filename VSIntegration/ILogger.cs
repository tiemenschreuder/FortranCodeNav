using System;

namespace VSIntegration
{
    public interface ILogger
    {
        void Error(string logMessage, Exception e, string context = null);
        void Write(string logMessage);
    }
}
using CodeSearcher.Interfaces;
using NLog;

namespace CodeSearcher
{
    class LoggerAdapter : ICodeSearcherLogger
    {
        private static ILogger m_Logger;

        public LoggerAdapter(ILogger logger)
        {
            m_Logger = logger;
        }
        public void Debug(string message, params object[] parameter)
        {
            m_Logger.Debug(message, parameter);
        }

        public void Error(string message, params object[] parameter)
        {
            m_Logger.Error(message, parameter);
        }

        public void Info(string message, params object[] parameter)
        {
            m_Logger.Info(message, parameter);
        }
    }
}

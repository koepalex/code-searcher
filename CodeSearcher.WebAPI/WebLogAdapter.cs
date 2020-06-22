using CodeSearcher.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodeSearcher.WebAPI
{
    public class WebLogAdapter : ICodeSearcherLogger
    {
        public WebLogAdapter(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        /// <inheritdoc />
        public void Debug(string message, params object[] parameter)
        {
            Logger.LogDebug(message, parameter);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] parameter)
        {
            Logger.LogError(message, parameter);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] parameter)
        {
            Logger.LogInformation(message, parameter);
        }
    }
}

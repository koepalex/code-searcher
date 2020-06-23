using CodeSearcher.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodeSearcher.WebAPI
{
    /// <summary>
    /// Adapter to wrap ILogger into CodeSearcherLogger interface
    /// </summary>
    public class WebLogAdapter : ICodeSearcherLogger
    {
        /// <summary>
        /// Default Constructor to create wrapper
        /// </summary>
        /// <param name="logger"></param>
        public WebLogAdapter(ILogger logger)
        {
            Logger = logger;
        }

        private ILogger Logger { get; }

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

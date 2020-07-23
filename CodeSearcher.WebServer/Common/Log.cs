using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CodeSearcher.WebServer
{
	public sealed class Log : IDisposable
	{
		private ILogger _logger;
		private static Log _self;
		private readonly ColoredConsoleTarget m_ConsoleTarget;
		private readonly FileTarget m_FileTarget;

		public Log()
		{
			// Step 1. Create configuration object 
			var config = new LoggingConfiguration();

			// Step 2. Create targets and add them to the configuration 
			m_ConsoleTarget = new ColoredConsoleTarget();
			config.AddTarget("console", m_ConsoleTarget);

			m_FileTarget = new FileTarget();
			config.AddTarget("file", m_FileTarget);

			// Step 3. Set target properties 
			m_ConsoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
			m_FileTarget.FileName = "${basedir}/codeSearcher.WebServer.log";
			m_FileTarget.Layout = "${message}";

			// Step 4. Define rules
			var rule1 = new LoggingRule("*", LogLevel.Info, m_ConsoleTarget);
			config.LoggingRules.Add(rule1);

			var rule2 = new LoggingRule("*", LogLevel.Warn, m_FileTarget);
			config.LoggingRules.Add(rule2);

			// Step 5. Activate the configuration
			LogManager.Configuration = config;

			_logger = LogManager.GetLogger("CodeSearcher.WebServer");
		}

		public static ILogger Get
		{
			get
			{
				if (_self == null)
				{
					_self = new Log();
				}

				return _self._logger;
			}
		}

		internal static Log GetInstance()
        {
			return _self;
        }

        public void Dispose()
        {
			m_ConsoleTarget?.Dispose();
			m_FileTarget?.Dispose();
			_self = null;
        }
    }
}


using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CodeSearcher.WebServer
{
	public class Log
	{
		private ILogger _logger;
		private static Log _self;

		public Log()
		{
			// Step 1. Create configuration object 
			var config = new LoggingConfiguration();

			// Step 2. Create targets and add them to the configuration 
			var consoleTarget = new ColoredConsoleTarget();
			config.AddTarget("console", consoleTarget);

			var fileTarget = new FileTarget();
			config.AddTarget("file", fileTarget);

			// Step 3. Set target properties 
			consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
			fileTarget.FileName = "${basedir}/codeSearcher.WebServer.log";
			fileTarget.Layout = "${message}";

			// Step 4. Define rules
			var rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget);
			config.LoggingRules.Add(rule1);

			var rule2 = new LoggingRule("*", LogLevel.Warn, fileTarget);
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
	}
}


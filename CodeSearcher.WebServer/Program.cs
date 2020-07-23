using System;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy;
using Nancy.Hosting.Self;

namespace CodeSearcher.WebServer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var cfgManager = ConfigManager.Get();
			if (!cfgManager.TryLoadConfig())
			{
				if (Log.Get.IsFatalEnabled)
					Log.Get.Fatal("error at config, exit program");
				
				Environment.ExitCode = -10;
				return;
			}

			var uri = cfgManager.Uri;

			if (Log.Get.IsInfoEnabled)
				Log.Get.Info("[Info] starting on {0}", uri.ToString());

			using (var nancyHost = new NancyHost(uri))
			{
				nancyHost.Start();

				if (Log.Get.IsInfoEnabled)
					Log.Get.Info("[Info] started");


				if (Environment.OSVersion.Platform == PlatformID.MacOSX
					|| Environment.OSVersion.Platform == PlatformID.Unix)
				{
					if (Log.Get.IsInfoEnabled)
						Log.Get.Info("Press Ctrl + c to exit");

					using var signint = new UnixSignal(Signum.SIGINT);
					using var signterm = new UnixSignal(Signum.SIGTERM);
					using var signquit = new UnixSignal(Signum.SIGQUIT);
					using var signhub = new UnixSignal(Signum.SIGHUP);
					UnixSignal.WaitAny(new[] {
						signint,
						signterm,
						signquit,
						signhub
					});
				}
				else
				{
					if (Log.Get.IsInfoEnabled)
						Log.Get.Info("Press any key to exit");
					
					Console.ReadKey();
				}
			}

			if (Log.Get.IsInfoEnabled)
				Log.Get.Info("[Info] CodeSearcher.Webserver stopped");

			Log.GetInstance().Dispose();
		}
	}
}

using System;
using CodeSearcher.WebServer.Models.CodeSearcher;
using Nancy;

namespace CodeSearcher.WebServer
{
	public class CodeSearcherModule : NancyModule
	{
		public CodeSearcherModule() 
		{
			var cfgManager = ConfigManager.Get();


			Get["/"] = _ =>
			{
				if (cfgManager.TryLoadConfig())
				{
					//TODO redirect to error page
				}
				return View[new SearchModel { IndexPath = cfgManager.IndexPath}];

			};
		}
	}
}


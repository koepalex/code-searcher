using System;
using CodeSearcher.WebServer.Models.CodeSearcher;
using Nancy;
using Nancy.ModelBinding;

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

				return View[new SearchModel { IndexPath = cfgManager.IndexPath }];
			};

			Get["/results"] = param =>
			{
				if (cfgManager.TryLoadConfig())
				{
					//TODO redirect to error page
				}

				var resultModel = this.Bind<ResultModel>();

				var searchManager = new SearchManager();
				searchManager.LookupSearchResults(cfgManager.IndexPath, resultModel);

				return View[resultModel];
			};
		}
	}
}


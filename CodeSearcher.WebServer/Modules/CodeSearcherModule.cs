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

			Get("/", _ =>
			{
				_ = cfgManager.TryLoadConfig();

				return View[new SearchModel { IndexPath = cfgManager.IndexPath, MaximumNumberOfHits = Int32.MaxValue }];
			});

			Get("/results", param =>
			{
				_ = cfgManager.TryLoadConfig();

				var resultModel = this.Bind<ResultModel>();

				var searchManager = new SearchManager();
				searchManager.LookupSearchResults(cfgManager.IndexPath, resultModel);

				return View[resultModel];
			});
		}
	}
}


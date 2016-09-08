using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;

namespace CodeSearcher.WebServer.Bootstrap
{
	public class NancyBootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			//activate that errors are shown on webpage
			StaticConfiguration.DisableErrorTraces = false;

			//log all requests which can't handled 
			pipelines.OnError.AddItemToEndOfPipeline((ctx, ex) =>
			{
				if (Log.Get.IsErrorEnabled)
				{
					Log.Get.Error(ex, "Unhandled error on request: " + ctx.Request.Url + " : " + ex.Message);
				}
				return ex;
			});

			base.ApplicationStartup(container, pipelines);
		}	

		protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
		{
			//define where static content (js, css, img) are located
			nancyConventions.StaticContentsConventions.Add(
				StaticContentConventionBuilder.AddDirectory("static", "static"));

			base.ConfigureConventions(nancyConventions);
		}
	}
}


using System;
using Nancy;
using Nancy.Conventions;

namespace CodeSearcher.WebServer.Bootstrap
{
	public class NancyBootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
		{
			//activate that errors are shown on webpage
			StaticConfiguration.DisableErrorTraces = false;

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


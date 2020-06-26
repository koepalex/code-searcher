using System;
using System.IO;
using System.Reflection;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CodeSearcher.WebAPI
{
#pragma warning disable CS1591
    public class Startup
    {
        private IWebHostEnvironment HostEnvironment { get; set; } 
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Code Searcher API",
                    Version = "v1",
                    Description = "REST based WebAPI to access code-searcher functionality",
                    License = new OpenApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://github.com/koepalex/code-searcher/blob/master/LICENSE")
                    },
                    Contact = new OpenApiContact
                    {
                        Url = new Uri("https://github.com/koepalex/code-searcher")
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "_docs", "gen", xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // don't slow down application while logging to console in production
            services.AddLogging(c =>
            {
                if(HostEnvironment.IsDevelopment())
                {
                    c.AddConsole();
                    c.SetMinimumLevel(LogLevel.Trace);
                }
                else
                {
                    c.SetMinimumLevel(LogLevel.Warning);
                }
                c.AddDebug();
                c.AddEventLog();
                
            });
            
            // use hangfire.io for background jobs
            services.AddHangfire(c => c.UseMemoryStorage());
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHangfireDashboard();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Searcher API V1");
                // serve swagger UI at app root route
                c.RoutePrefix = string.Empty;
            });
            
            
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CS1591
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace GatewayApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
       // public Startup(IHostingEnvironment env)
        //{
            //var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            //builder.SetBasePath(env.ContentRootPath)
            //       //add configuration.json  
            //       .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
            //       .AddEnvironmentVariables();

            //Configuration = builder.Build();
       // }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot();
            services.AddHealthChecks();
          // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("healthy");
                });
            });
            app.UseOcelot().Wait();
           // app.UseMvc();
            //  app.UseMvc();
            app.UseHealthChecks("/health");
              
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}

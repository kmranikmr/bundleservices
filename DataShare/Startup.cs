using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataShareService.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using DataShareService.Controller;

namespace DataShareService
{
    public class DatabaseConfig
    {
        public string postgres { get; set; }
        public string workflowServer { get; set; }
        public string milvusServer { get; set; }
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }
    

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
             services.AddHealthChecks();
            // services.AddMvc(options => {
            //an instant  
            //  options.Filters.Add(new ApiKeyAuthAttribute());
            //By the type  
            //   options.Filters.Add(typeof(ApiKeyAuthAttribute));
            // });
            //services.AddMvc(config =>
            //{
            //    config.Filters.Add(new ApiKeyAuthAttribute());
            //});
            // services.add(options =>
            // {
            //     options.Filters.Add(typeof(MySampleActionFilter));
            // });
            services.Configure<DatabaseConfig>(Configuration.GetSection("database"));
            var workflowConnectionString = Configuration.GetConnectionString("workflowServer");
            var milvusConnectionString = Configuration.GetConnectionString("milvusServer");
            services.AddMvc(options => {
                    //an instant  
                    options.Filters.Add(new ApiKeyAttributeFilter());
                //    //By the type  
                    options.Filters.Add(typeof(ApiKeyAttributeFilter));
                });
            //services.Configure<string>(option => option = workflowConnectionString);
           // services.Configure<ConnectionStringsConfig>(option => option.MilvusServiceConnection = milvusConnectionString);
            services.AddScoped<ApiKeyAttributeFilter>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseAuthentication();
           app.UseMvc();
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
             app.UseHealthChecks("/health");
        }
    }
}

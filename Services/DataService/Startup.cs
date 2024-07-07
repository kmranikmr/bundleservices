using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using AutoMapper;
using DataAccess.DTO;
using System.IO;
using Microsoft.AspNetCore.Http;
using NLog.Extensions.Logging;
using NLog.Web;
//using DataAccess.Models;
using Microsoft.AspNetCore.HttpOverrides;

namespace DataService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            //services.AddHttpsRedirection(options =>
            //{
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //    options.HttpsPort = 6002;
            //});
            services.ConfigureAuth(Configuration);

            services.AddAutoMapper(cfg =>
            {

               
                cfg.CreateMap<SchemaDTO, ProjectSchema>();
               
                cfg.CreateMap<SchemaModelDTO, SchemaModel>();
               
                cfg.CreateMap<ModelMetadataDTO, ModelMetadata>();
                cfg.CreateMap<ProjectFileDTO, ProjectFile>();
         
                cfg.CreateMap<ReaderDTO, Reader>();
                cfg.CreateMap<WriterDTO, Writer>();
                cfg.CreateMap<WriterTypeDTO, WriterType>();
                cfg.CreateMap<SearchHistoryDTO, SearchHistory>();
                cfg.CreateMap<SearchGraphDTO, SearchGraph>();
                cfg.CreateMap<JobDTO, Job>();
               
                cfg.CreateMap<SchemaTreeDTO, ProjectSchema>();
                cfg.CreateMap<ModelTreeDTO, SchemaModel>();
                cfg.CreateMap<WorkflowProjectDTO, WorkflowProject>();
                cfg.CreateMap<WorkflowSessionAttemptDTO, WorkflowSessionAttempt>();
                cfg.CreateMap<WorkflowVersionDTO, WorkflowVersion>();
                cfg.CreateMap<WorkflowSessionLogDTO, WorkflowSessionLog>();
                cfg.CreateMap<WorkflowServerTypeDTO, WorkflowServerType>();
                cfg.CreateMap<WorkflowTestDTO, WorkflowTest>();
            }, AppDomain.CurrentDomain.GetAssemblies());

            services.ConfigureSwagger();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connectionString = Configuration.GetConnectionString("localDb");
            var workflowConnectionString = Configuration.GetConnectionString("workflowServer");
            var queryServiceString = Configuration.GetConnectionString("queryService");

            services.Configure<ConnectionStringsConfig>(option => option.DefaultConnection = connectionString);
            //var connection = @"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True;ConnectRetryCount=0";
           
             services.Configure<ConnectionStringsConfig>(option => option.WorkflowConnection = workflowConnectionString);
            services.Configure<ConnectionStringsConfig>(option => option.QueryServiceConnection = queryServiceString);

            services.AddDbContext<DAPDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IRepository, Repository>();
            //using (StreamReader reader = new StreamReader("testworkflow.json"))
            //{
            //    var json = reader.ReadToEnd();
            //    var obj = NodeRepository.Test(json, 2);
            //}
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)// ILoggingBuilder loggerFactory)
        {

            app.UseAuthentication();
            // loggerFactory.AddNLog();
            //  app.AddNLogWeb();
            //add NLog.Web
            app.UseHttpsRedirection();
            app.UseConfiguredSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            //});
            // app.UseCors(builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials(); });
            app.UseCors("CorsPolicy");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }            
           
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

              app.UseHealthChecks("/health");
        }
    }
}

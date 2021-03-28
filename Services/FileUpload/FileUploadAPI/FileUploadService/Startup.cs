using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileUploadService.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FileUploadService.Model;
using Microsoft.AspNetCore.Http.Features;
using FileUploadService.Models;
using AutoMapper;

namespace FileUploadService
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
            services.ConfigureAuth(Configuration);
            services.ConfigureSwagger();
            services.AddAutoMapper(cfg =>
            {


                cfg.CreateMap<CreatePostRequest, Models.ProjectFile>();

                
            }, AppDomain.CurrentDomain.GetAssemblies());
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            // var connstr = Configuration.GetConnectionString("");
            //services.Configure<string>(connstr);
            services.Configure<FormOptions>(x =>
            {
               
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
            services.AddSignalR();
            var connectionString = Configuration.GetConnectionString("localDb");

            //var connection = @"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True;ConnectRetryCount=0";
            services.AddDbContext<ProjectFileContextDB>(options => options.UseSqlServer(connectionString));

            // services.AddDbContext<ProjectFileDbContext>(options =>
            //      options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressMapClientErrors = true;
                 });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseConfiguredSwagger();
                app.UseCors(
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    }
                    );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSignalR(routes =>
            {

                routes.MapHub<ProgressHub>("/progress");
            });
             app.UseHealthChecks("/health");
            //app.UseStaticFiles(); // For the wwwroot folder

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //        Path.Combine(Directory.GetCurrentDirectory(), "FileUploadService/StaticFiles")),
            //    RequestPath = "/StaticFiles"
            //});
            app.UseMvc();
        }
    }
}

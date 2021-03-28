using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using NLog.Web.AspNetCore;
namespace DataService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //try
            //{
            //    SchemaDTO testObj = DTO.SchemaDTO.GetSampleObject(1);
            //    var config = new MapperConfiguration(cfg =>
            //    {

            //        cfg.CreateMap<SchemaDTO, ProjectSchema>();
            //        cfg.CreateMap<SchemaModelDTO, SchemaModel>();                    
            //        cfg.CreateMap<ModelMetadataDTO, ModelMetadata>();
            //    });
            //    var mapper = config.CreateMapper();

            //    var project = mapper.Map<ProjectSchema>(testObj);

            //    var json = JsonConvert.SerializeObject(DTO.SchemaDTO.GetSampleObject(1));
            //}
            //catch (Exception ex)
            //{
            //    "tet".ToString();
            //    //throw;
            //}
            Logger logger;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            else
             
                logger = NLogBuilder.ConfigureNLog("nloglinux.config").GetCurrentClassLogger();
            logger.Debug("init main");

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
           .UseStartup<Startup>().UseUrls("http://0.0.0.0:6002")
           .ConfigureLogging(logger => { logger.ClearProviders(); logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); })
           .UseNLog();
        //"https://0.0.0.0:6002", "https://localhost:6002
        //"http://0.0.0.0:6002", "http://localhost:6002"
    }
}

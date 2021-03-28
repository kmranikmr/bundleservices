using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace GatewayApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
            .ConfigureAppConfiguration( (host, config)=>
            {
                config.AddJsonFile(@"ocelot.json");

            })
            .ConfigureLogging(logger => { logger.ClearProviders(); logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); })
           .UseNLog()
                 // .ConfigureLogging(logger=>logger.AddConsole())
                 .UseStartup<Startup>().UseUrls("http://0.0.0.0:6010")
                   ;
                
    }
}

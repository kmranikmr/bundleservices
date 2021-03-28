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

namespace FileUploadService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            Logger logger;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            else

                logger = NLogBuilder.ConfigureNLog("nloglinux.config").GetCurrentClassLogger();
            logger.Debug("init main");
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
            .ConfigureLogging(logger => { logger.ClearProviders(); logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information); })
           .UseNLog()
                .UseUrls("http://0.0.0.0:6001");
    }
}

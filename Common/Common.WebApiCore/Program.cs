/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Common.WebApiCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)

               .UseStartup<Startup>()
               .UseUrls("http://0.0.0.0:5000","https://0.0.0.0:5001");

    }
}

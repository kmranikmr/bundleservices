/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Common.DataAccess.EFCore
{
    class DesignTimeContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        // this class used by EF tool for creating migrations via Package Manager Console
        // Add-Migration MigrationName -Project Common.DataAccess.EFCore -StartupProject Common.DataAccess.EFCore
        // or in case of console building: dotnet ef migrations add MigrationName --project Common.DataAccess.EFCore --startup-project Common.DataAccess.EFCore
        // after migration is added, please add migrationBuilder.Sql(SeedData.Initial()); to the end of seed method for initial data
        public DesignTimeContextFactory() {}

        public DataContext CreateDbContext(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Common.WebApiCore"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("localDb");

            var builder = new DbContextOptionsBuilder<DataContext>();
            builder.UseNpgsql(connectionString);

            return new DataContext(builder.Options);
        }
    }
}

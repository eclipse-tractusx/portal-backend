using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CatenaX.NetworkServices.UserAdministration.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Kubernetes"))
                    {
                        var provider = new PhysicalFileProvider("/app/secrets");
                        builder.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: false);
                    }
                })
                .ConfigureAppConfiguration(appConfiguration => {
                    foreach(var source in appConfiguration.Sources) {
                        var fileSource = source as FileConfigurationSource;
                        if (fileSource != null) {
                            fileSource.ReloadOnChange = false;
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

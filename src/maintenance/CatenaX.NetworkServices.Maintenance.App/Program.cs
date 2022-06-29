// See https://aka.ms/new-console-template for more information

using System.Reflection;
using CatenaX.NetworkServices.Maintenance.App;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(cfg =>
    {
        // Read configuration for configuring logger.
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        // Build a config object, using env vars and JSON providers.
        if (environmentName == "Kubernetes")
        {
            var provider = new PhysicalFileProvider("/app/secrets");
            cfg.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: true);
        }

        cfg.AddUserSecrets(Assembly.GetExecutingAssembly());
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<PortalDbContext>(o =>
            o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb")));
        services.AddHostedService<BatchDeleteService>();
    }).Build();

host.Run();

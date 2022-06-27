// See https://aka.ms/new-console-template for more information

using System.Reflection;
using CatenaX.NetworkServices.Maintenance.App;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<PortalDbContext>(o =>
            o.UseNpgsql(hostContext.Configuration.GetConnectionString("PortalDb")));
        services.AddHostedService<BatchDeleteService>();
    })
    .ConfigureHostConfiguration(cfg =>
    {
        // Read configuration for configuring logger.
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        cfg.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly());

        // Build a config object, using env vars and JSON providers.
        if (environmentName != "Kubernetes") return;

        var provider = new PhysicalFileProvider("/app/secrets");
        cfg.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: true);
    }).Build();

host.RunAsync();

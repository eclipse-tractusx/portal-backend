// See https://aka.ms/new-console-template for more information

using System.Reflection;
using CatenaX.NetworkServices.Maintenance.App;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

var environmentName = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");
// Build a config object, using env vars and JSON providers.
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environmentName}.json", true, true)
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());

if (environmentName == "Kubernetes")
{
    var provider = new PhysicalFileProvider("/app/secrets");
    configBuilder.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: true);
}

var configuration = configBuilder.Build();

var host = Host.CreateDefaultBuilder(args)
    .Build();
var services = new ServiceCollection();
services.AddDbContext<PortalDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("PortalDb")));
ServiceProvider serviceProvider = services.BuildServiceProvider();

BatchDeleteService.BatchDeleteDocuments(serviceProvider.GetRequiredService<PortalDbContext>(), configuration.GetValue<int>("DeleteIntervalInDays"));

host.RunAsync();
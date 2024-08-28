using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;

/// <summary>
/// Settings for the <see cref="BatchDeleteService"/>
/// </summary>
public class BatchDeleteServiceSettings
{
    /// <summary>
    /// Documents older than this configured value will be deleted
    /// </summary>
    [Required]
    public int DeleteIntervalInDays { get; set; }
}

/// <summary>
/// Extensions for the <see cref="BatchDeleteService"/>
/// </summary>
public static class BatchDeleteServiceExtensions
{
    /// <summary>
    /// Adds the <see cref="BatchDeleteService"/> to the service collection
    /// </summary>
    /// <param name="services">The service collection used for di</param>
    /// <param name="section">The configuration section to get the settings from</param>
    /// <returns>The enhanced service collection</returns>
    public static IServiceCollection AddBatchDelete(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<BatchDeleteServiceSettings>().Bind(section);
        services
            .AddTransient<IBatchDeleteService, BatchDeleteService>();
        return services;
    }
}

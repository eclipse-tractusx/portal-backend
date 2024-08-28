using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;

/// <summary>
/// Extension methods to register the necessary services for the maintenance job
/// </summary>
public static class MaintenanceServiceExtensions
{
    /// <summary>
    /// Adds the dependencies for the maintenance service
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The enhanced service collection</returns>
    public static IServiceCollection AddMaintenanceService(this IServiceCollection services) =>
        services
            .AddTransient<MaintenanceService>()
            .AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
}

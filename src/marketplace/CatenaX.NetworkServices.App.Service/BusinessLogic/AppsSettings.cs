using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Framework.ErrorHandling;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// Config Settings for Apps
/// </summary>
public class AppsSettings
{
    /// <summary>
    /// Constructor
    /// </summary>
    public AppsSettings()
    {
        CompanyAdminRoles = null!;
        NotificationTypeIds = null!;
    }
    
    /// <summary>
    /// Company Admin Roles
    /// </summary>
    /// <value></value>
    public IDictionary<string,IEnumerable<string>> CompanyAdminRoles { get; set; }

    /// <summary>
    /// Notification Type Id
    /// </summary>
    /// <value></value>
    public IEnumerable<NotificationTypeId> NotificationTypeIds { get; set; }

}

/// <summary>
/// App Setting Extension class
/// </summary>
public static class AppsSettingsExtension
{
    /// <summary>
    /// Method to Configure Apps Settings
    /// </summary>
    /// <param name="services"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureAppsSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<AppsSettings>(x =>
            {
                section.Bind(x);
                if (x.CompanyAdminRoles == null)
                {
                    throw new ConfigurationException($"{nameof(AppsSettings)}: {nameof(x.CompanyAdminRoles)} must not be null");
                }
                if (x.NotificationTypeIds == null)
                {
                    throw new ConfigurationException($"{nameof(AppsSettings)}: {nameof(x.NotificationTypeIds)} must not be null");
                }
            });
}
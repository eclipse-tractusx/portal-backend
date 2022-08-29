using CatenaX.NetworkServices.Framework.ErrorHandling;
namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Settings used in business logic concerning service account.
/// </summary>
public class ServiceAccountSettings
{
    public ServiceAccountSettings() 
    {
        ClientId = null!;
    }

    /// <summary>
    /// Service account clientId.
    /// </summary>
    public string ClientId { get; set; }
}

public static class ServiceAccountSettingsExtensions
{
    public static IServiceCollection ConfigureServiceAccountSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<ServiceAccountSettings>()
            .Bind(section)
            .Validate(x =>
            {
                if (string.IsNullOrWhiteSpace(x.ClientId))
                    throw new ConfigurationException($"{nameof(ServiceAccountSettings)}: {nameof(x.ClientId)} must not be null or empty");

                return true;
            })
            .ValidateOnStart();
        return services;
    }
}

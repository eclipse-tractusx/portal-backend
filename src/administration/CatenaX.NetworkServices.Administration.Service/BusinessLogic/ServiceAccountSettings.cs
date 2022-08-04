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
        return services.Configure<ServiceAccountSettings>(x =>
        {
            section.Bind(x);
            if (String.IsNullOrWhiteSpace(x.ClientId))
            {
                throw new Exception($"{nameof(ServiceAccountSettings)}: {nameof(x.ClientId)} must not be null or empty");
            }
        });

    }
}

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Settings used in business logic concerning connectors.
/// </summary>
public class ConnectorsSettings
{
    public ConnectorsSettings() 
    {
        SdFactoryUrl = string.Empty;
    }

    public ConnectorsSettings(string sdFactoryUrl)
    {
        SdFactoryUrl = sdFactoryUrl;
    }

    /// <summary>
    /// Maximum amount of entries per page in paginated connector lists.
    /// </summary>
    public int MaxPageSize { get; set; }

    /// <summary>
    /// SD Factory endpoint for registering connectors.
    /// </summary>
    public string SdFactoryUrl { get; set; }
}

public static class ConnectorsSettingsExtensions
{
    public static IServiceCollection ConfigureConnectorsSettings(
        this IServiceCollection services,
        IConfigurationSection section
        )
    {
        return services.Configure<ConnectorsSettings>(x => section.Bind(x));
    }
}

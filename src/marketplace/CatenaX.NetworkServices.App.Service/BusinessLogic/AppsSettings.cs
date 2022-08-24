namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// settings used in business logic concerning apps.
/// </summary>
public class AppsSettings
{
    public AppsSettings()
    {
        BasePortalAddress = null!;
    }
    /// <summary>
    /// base portal address for subscription request url
    /// </summary>
    public string BasePortalAddress { get; set; }
}

public static class AppsSettingsExtension
{
    public static IServiceCollection ConfigureAppsSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<AppsSettings>(x =>
            {
                section.Bind(x);

                if (String.IsNullOrWhiteSpace(x.BasePortalAddress))
                {
                    throw new Exception($"{nameof(AppsSettings)}: {nameof(x.BasePortalAddress)} must not be null or empty");
                }
            });
}

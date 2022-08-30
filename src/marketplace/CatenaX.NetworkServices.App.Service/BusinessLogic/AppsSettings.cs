using CatenaX.NetworkServices.Framework.ErrorHandling;
namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

public class AppsSettings
{
    public string BasePortalAddress { get; init; } = null!;
    public void Validate()
    {
        new ConfigurationValidation<AppsSettings>()
        .NotNullOrWhiteSpace(BasePortalAddress, () => nameof(BasePortalAddress));
    }
}


public static class AppsSettingsExtension
{
    public static IServiceCollection ConfigureAppsSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<AppsSettings>(x =>
            {
                section.Bind(x);
                x.Validate();
            });
}

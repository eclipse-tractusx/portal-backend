namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        KeyCloakClientID = null!;
        BasePortalAddress = null!;
    }

    public string KeyCloakClientID { get; set; }
    public string BasePortalAddress { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<RegistrationSettings>(x =>
            {
                section.Bind(x);
                if (String.IsNullOrWhiteSpace(x.KeyCloakClientID))
                {
                    throw new Exception($"{nameof(RegistrationSettings)}: {nameof(x.KeyCloakClientID)} must not be null or empty");
                }
                if (String.IsNullOrWhiteSpace(x.BasePortalAddress))
                {
                    throw new Exception($"{nameof(RegistrationSettings)}: {nameof(x.BasePortalAddress)} must not be null or empty");
                }
            });
}

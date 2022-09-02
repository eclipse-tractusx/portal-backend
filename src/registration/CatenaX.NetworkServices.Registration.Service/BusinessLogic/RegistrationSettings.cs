using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        KeyCloakClientID = null!;
        BasePortalAddress = null!;
    }

    [Required(AllowEmptyStrings = false)]
    public string KeyCloakClientID { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<RegistrationSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}

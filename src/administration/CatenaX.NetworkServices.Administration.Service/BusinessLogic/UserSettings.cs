using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class UserSettings
{
    public UserSettings()
    {
        Portal = null!;
        PasswordReset = null!;
    }

    [Required]
    public UserSetting Portal { get; set; }
    public PasswordReset PasswordReset { get; set; }
    public int ApplicationsMaxPageSize { get; set; }
}

public class UserSetting
{
    public UserSetting()
    {
        KeyCloakClientID = null!;
        BasePortalAddress = null!;
    }

    [Required(AllowEmptyStrings = false)]
    public string KeyCloakClientID { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string BasePortalAddress { get; set; }
}

public class PasswordReset
{
    public int NoOfHours { get; set; }
    public int MaxNoOfReset { get; set; }
}

public static class UserSettingsExtension
{
    public static IServiceCollection ConfigureUserSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<UserSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}

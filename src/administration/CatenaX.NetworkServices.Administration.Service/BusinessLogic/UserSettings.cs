namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class UserSettings
{
    public UserSettings()
    {
        Portal = null!;
        PasswordReset = null!;
    }

    public UserSetting Portal { get; set; }
    public PasswordReset PasswordReset { get; set; }
    
}

public class UserSetting
{
    public UserSetting()
    {
        KeyCloakClientID = null!;
        BasePortalAddress = null!;
    }

    public string KeyCloakClientID { get; set; }
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
        IConfigurationSection section) =>
        services.Configure<UserSettings>(x =>
            {
                section.Bind(x);
                if (x.Portal == null)
                {
                    throw new Exception($"{nameof(UserSettings)}: {nameof(x.Portal)} must not be null");
                }
                if (String.IsNullOrWhiteSpace(x.Portal.KeyCloakClientID))
                {
                    throw new Exception($"{nameof(UserSettings)}: {nameof(x.Portal.KeyCloakClientID)} must not be null or empty");
                }
                if (String.IsNullOrWhiteSpace(x.Portal.BasePortalAddress))
                {
                    throw new Exception($"{nameof(UserSettings)}: {nameof(x.Portal.BasePortalAddress)} must not be null or empty");
                }
                if (x.PasswordReset == null)
                {
                    throw new Exception($"{nameof(UserSettings)}: {nameof(x.PasswordReset)} must not be null");
                }
            });
}

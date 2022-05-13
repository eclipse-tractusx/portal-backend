namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class InvitationSettings
{
    public string RegistrationAppAddress { get; set; }
    public IDictionary<string,IEnumerable<string>> InvitedUserInitialRoles { get; set; }
}

public static class InvitationSettingsExtension
{
    public static IServiceCollection ConfigureInvitationSettings(
        this IServiceCollection services,
        IConfigurationSection section
        )
    {
        return services.Configure<InvitationSettings>(x => section.Bind(x));
    }
}


namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class InvitationSettings
{
    public InvitationSettings()
    {
        RegistrationAppAddress = null!;
        InvitedUserInitialRoles = null!;
    }
    public string RegistrationAppAddress { get; set; }
    public IDictionary<string,IEnumerable<string>> InvitedUserInitialRoles { get; set; }
}

public static class InvitationSettingsExtension
{
    public static IServiceCollection ConfigureInvitationSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<InvitationSettings>(x =>
            {
                section.Bind(x);

                if (String.IsNullOrWhiteSpace(x.RegistrationAppAddress))
                {
                    throw new Exception($"{nameof(InvitationSettings)}: {nameof(x.RegistrationAppAddress)} must not be null or empty");
                }
                if (x.InvitedUserInitialRoles == null)
                {
                    throw new Exception($"{nameof(InvitationSettings)}: {nameof(x.InvitedUserInitialRoles)} must not be null");
                }
            });
}

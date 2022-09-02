using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class InvitationSettings
{
    public InvitationSettings()
    {
        RegistrationAppAddress = null!;
        InvitedUserInitialRoles = null!;
    }
    
    [Required(AllowEmptyStrings = false)]
    public string RegistrationAppAddress { get; set; }
    
    [Required]
    public IDictionary<string,IEnumerable<string>> InvitedUserInitialRoles { get; set; }
}

public static class InvitationSettingsExtension
{
    public static IServiceCollection ConfigureInvitationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<InvitationSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}

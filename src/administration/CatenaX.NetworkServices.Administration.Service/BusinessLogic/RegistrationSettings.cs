using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        ApplicationApprovalInitialRoles = null!;
        PartnerUserInitialRoles = null!;
        CompanyAdminRoles = null!;
        WelcomeNotificationTypeIds = null!;
    }

    public int ApplicationsMaxPageSize { get; set; }
    
    [Required]
    public IDictionary<string,IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
    [Required]
    public IDictionary<string,IEnumerable<string>> PartnerUserInitialRoles { get; set; }
    [Required]
    public IDictionary<string,IEnumerable<string>> CompanyAdminRoles { get; set; }

    /// <summary>
    /// IDs of the notification types that should be created as welcome notifications
    /// </summary>
    [Required]
    public IEnumerable<NotificationTypeId> WelcomeNotificationTypeIds { get; set; }
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

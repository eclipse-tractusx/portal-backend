using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        ApplicationApprovalInitialRoles = null!;
        PartnerUserInitialRoles = null!;
        CatenaXCompanyName = null!;
        CxAdminRolename = null!;
        CompanyAdminRole = null!;
        WelcomeNotificationTypeIds = null!;
    }

    public int ApplicationsMaxPageSize { get; set; }
    public IDictionary<string, IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
    public IDictionary<string,IEnumerable<string>> PartnerUserInitialRoles { get; set; }

    /// <summary>
    /// Company name of the Catena X Company
    /// </summary>
    public string CatenaXCompanyName { get; set; }

    /// <summary>
    /// Name of the CX Admin Role
    /// </summary>
    public string CxAdminRolename { get; set; }

    /// <summary>
    /// Name of the Company Admin
    /// </summary>
    public string CompanyAdminRole { get; set; }

    /// <summary>
    /// IDs of the notification types that should be created as welcome notifications
    /// </summary>
    public IEnumerable<NotificationTypeId> WelcomeNotificationTypeIds { get; set; }
}

public static class RegistrationSettingsExtension
{
    public static IServiceCollection ConfigureRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section) =>
        services.Configure<RegistrationSettings>(x =>
            {
                section.Bind(x);
                if (x.ApplicationApprovalInitialRoles == null)
                {
                    throw new UnexpectedConditionException($"{nameof(RegistrationSettings)}: {nameof(x.ApplicationApprovalInitialRoles)} must not be null");
                }
                if (x.PartnerUserInitialRoles == null)
                {
                    throw new UnexpectedConditionException($"{nameof(RegistrationSettings)}: {nameof(x.PartnerUserInitialRoles)} must not be null");
                }
                if (x.WelcomeNotificationTypeIds == null)
                {
                    throw new UnexpectedConditionException($"{nameof(RegistrationSettings)}: {nameof(x.WelcomeNotificationTypeIds)} must not be null");
                }
            });
}

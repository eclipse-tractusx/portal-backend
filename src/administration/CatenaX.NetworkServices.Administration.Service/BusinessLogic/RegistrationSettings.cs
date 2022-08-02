using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public class RegistrationSettings
{
    public RegistrationSettings()
    {
        ApplicationApprovalInitialRoles = null!;
        PartnerUserInitialRoles = null!;
        WelcomeNotificationTypeIds = null!;
    }

    public int ApplicationsMaxPageSize { get; set; }
    public IDictionary<string, IEnumerable<string>> ApplicationApprovalInitialRoles { get; set; }
    public IDictionary<string,IEnumerable<string>> PartnerUserInitialRoles { get; set; }

    /// <summary>
    /// Company name of the Catena X Company
    /// </summary>
    public Guid CatenaXCompanyId { get; set; }

    /// <summary>
    /// Name of the CX Admin Role
    /// </summary>
    public Guid CxAdminRoleId { get; set; }

    /// <summary>
    /// Name of the Company Admin
    /// </summary>
    public Guid CompanyAdminRoleId { get; set; }

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
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.ApplicationApprovalInitialRoles)} must not be null");
                }
                if (x.PartnerUserInitialRoles == null)
                {
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.PartnerUserInitialRoles)} must not be null");
                }
                if (x.CatenaXCompanyId == Guid.Empty)
                {
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.CatenaXCompanyId)} must not be null");
                }
                if (x.CxAdminRoleId == Guid.Empty)
                {
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.CxAdminRoleId)} must not be empty");
                }
                if (x.CompanyAdminRoleId == Guid.Empty)
                {
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.CompanyAdminRoleId)} must not be empty");
                }
                if (x.WelcomeNotificationTypeIds == null)
                {
                    throw new ConfigurationException($"{nameof(RegistrationSettings)}: {nameof(x.WelcomeNotificationTypeIds)} must not be empty");
                }
            });
}

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// App subscription relationship between companies and apps.
/// </summary>
public class CompanyAssignedApp
{
    private CompanyAssignedApp() {
        AppSubscriptionStatusId = AppSubscriptionStatusId.PENDING;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appId">App id.</param>
    /// <param name="companyId">Company id.</param>
    /// <param name="appSubscriptionStatusId">app subscription status.</param>
    public CompanyAssignedApp(Guid appId, Guid companyId) : this()
    {
        AppId = appId;
        CompanyId = companyId;
    }

    /// <summary>
    /// ID of the company subscribing an app.
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// ID of the apps subscribed by a company.
    /// </summary>
    public Guid AppId { get; private set; }

    /// <summary>
    /// ID of the app subscription status.
    /// </summary>
    public AppSubscriptionStatusId AppSubscriptionStatusId { get; set; }

    // Navigation properties
    /// <summary>
    /// Subscribed app.
    /// </summary>
    public virtual App? App { get; private set; }
    /// <summary>
    /// Subscribing company.
    /// </summary>
    public virtual Company? Company { get; private set; }
    /// <summary>
    /// Subscription status.
    /// </summary>
    public virtual AppSubscriptionStatus? AppSubscriptionStatus { get; private set; }
}

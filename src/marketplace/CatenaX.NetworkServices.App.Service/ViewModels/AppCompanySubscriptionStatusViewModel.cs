namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// View model containing an app id and connected company subscription statuses.
/// </summary>
public class AppCompanySubscriptionStatusViewModel
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public AppCompanySubscriptionStatusViewModel()
    {
        CompanySubscriptionStatuses = new HashSet<CompanySubscriptionStatusViewModel>();
    }

    /// <summary>
    /// Id of the app.
    /// </summary>
    public Guid AppId { get; set; }

    /// <summary>
    /// Subscription statuses of subscribing companies.
    /// </summary>
    public ICollection<CompanySubscriptionStatusViewModel> CompanySubscriptionStatuses { get; set; }
}

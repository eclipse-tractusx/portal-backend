namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// View model of an application's detailed data.
/// </summary>
public class AppDetailsViewModel
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <param name="leadPictureUri">Lead picture URI.</param>
    /// <param name="providerUri">Provider URI.</param>
    /// <param name="provider">Provider.</param>
    /// <param name="longDescription">Long description.</param>
    /// <param name="price">Price.</param>
    public AppDetailsViewModel(string title, string leadPictureUri, string providerUri, string provider, string longDescription, string price)
    {
        Title = title;
        LeadPictureUri = leadPictureUri;
        DetailPictureUris = new List<string>();
        ProviderUri = providerUri;
        Provider = provider;
        UseCases = new List<string>();
        LongDescription = longDescription;
        Price = price;
        Tags = new List<string>();
    }

    /// <summary>
    /// ID of the app.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; set; }

    /// <summary>
    /// List of URIs to app's secondary pictures.
    /// </summary>
    public IEnumerable<string> DetailPictureUris { get; set; }

    /// <summary>
    /// Uri to provider's marketing presence.
    /// </summary>
    public string ProviderUri { get; set; }

    /// <summary>
    /// Provider of the app.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Email address of the app's primary contact.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Phone number of the app's primary contact.
    /// </summary>
    public string? ContactNumber { get; set; }

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    public IEnumerable<string> UseCases { get; set; }

    /// <summary>
    /// Long description of the app.
    /// </summary>
    public string LongDescription { get; set; }

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; }

    /// <summary>
    /// Tags assigned to application.
    /// </summary>
    public IEnumerable<string> Tags { get; set; }

    /// <summary>
    /// Whether app has been purchased by the user's company.
    /// </summary>
    public bool? IsSubscribed { get; set; }
}

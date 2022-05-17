namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// View model of an application's base data.
/// </summary>
public class AppViewModel
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <param name="shortDescription">Short description.</param>
    /// <param name="provider">Provider.</param>
    /// <param name="price">Price.</param>
    /// <param name="leadPictureUri">Lead pircture URI.</param>
    public AppViewModel(string title, string shortDescription, string provider, string price, string leadPictureUri)
    {
        Title = title;
        ShortDescription = shortDescription;
        Provider = provider;
        UseCases = new List<string>();
        Price = price;
        LeadPictureUri = leadPictureUri;
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
    /// Short description of the app.
    /// </summary>
    public string ShortDescription { get; set; }

    /// <summary>
    /// Provider of the app.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    public IEnumerable<string> UseCases { get; set; }

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; }

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; set; }
}

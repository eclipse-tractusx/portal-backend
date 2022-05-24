namespace CatenaX.NetworkServices.App.Service.ViewModels;

/// <summary>
/// Basic model for data needed in business application display.
/// </summary>
public class BusinessAppViewModel
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">App name.</param>
    /// <param name="uri">App uri.</param>
    /// <param name="leadPictureUri">Lead picture uri.</param>
    /// <param name="provider">App provider.</param>
    public BusinessAppViewModel(string name, string uri, string leadPictureUri, string provider)
    {
        Name = name;
        Uri = uri;
        LeadPictureUri = leadPictureUri;
        Provider = provider;
    }

    /// <summary>
    /// ID of the application.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the application.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Access uri of the application.
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Uri of the app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; set; }

    /// <summary>
    /// Provider of the application.
    /// </summary>
    public string Provider { get; set; }
}

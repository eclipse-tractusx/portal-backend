namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// View model of an application's base data.
/// </summary>
public class AppData
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <param name="shortDescription">Short description.</param>
    /// <param name="provider">Provider.</param>
    /// <param name="price">Price.</param>
    /// <param name="leadPictureUri">Lead pircture URI.</param>
    public AppData(string title, string shortDescription, string provider, string price, string leadPictureUri)
    {
        Title = title;
        ShortDescription = shortDescription;
        Provider = provider;
        UseCases = new List<string>();
        Price = price;
        LeadPictureUri = leadPictureUri;
    }

    public Guid Id { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Provider { get; init; }
    public string Price { get; init; }
    public string LeadPictureUri { get; init; }
    public IEnumerable<string> UseCases { get; init; }

}
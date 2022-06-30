namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// View model of an application's base data.
/// </summary>
public record AppData(Guid Id, string Title, string ShortDescription, string Provider, string Price, string LeadPictureUri, IEnumerable<string> UseCases)
{
    /// <summary>
    /// ID of the app.
    /// </summary>
    public Guid Id { get; set; } = Id;

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    public string Title { get; set; } = Title;

    /// <summary>
    /// Short description of the app.
    /// </summary>
    public string ShortDescription { get; set; } = ShortDescription;

    /// <summary>
    /// Provider of the app.
    /// </summary>
    public string Provider { get; set; } = Provider;

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    public IEnumerable<string> UseCases { get; set; } = UseCases;

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; } = Price;

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; set; } = LeadPictureUri;
}

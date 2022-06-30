namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Basic model for data needed in business application display.
/// </summary>
public record BusinessAppData(Guid Id, string Name, string Uri, string LeadPictureUri, string Provider)
{
    /// <summary>
    /// ID of the application.
    /// </summary>
    public Guid Id { get; } = Id;

    /// <summary>
    /// Name of the application.
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// Access uri of the application.
    /// </summary>
    public string Uri { get; } = Uri;

    /// <summary>
    /// Uri of the app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; } = LeadPictureUri;

    /// <summary>
    /// Provider of the application.
    /// </summary>
    public string Provider { get; } = Provider;
}

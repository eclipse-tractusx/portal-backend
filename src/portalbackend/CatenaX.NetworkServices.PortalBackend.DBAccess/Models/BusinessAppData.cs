namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Basic model for data needed in business application display.
/// </summary>
public record BusinessAppData(Guid Id, string Name, string Uri, string LeadPictureUri, string Provider);

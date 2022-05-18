namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class IdpUser
{
    public string? TargetIamUserId { get; set; }
    public string? IdpName { get; set; }
    public Guid CompanyId { get; set; }
}

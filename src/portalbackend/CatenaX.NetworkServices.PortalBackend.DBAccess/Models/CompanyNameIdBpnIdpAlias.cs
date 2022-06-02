namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyNameIdIdpAlias
{
    public CompanyNameIdIdpAlias(string companyName, Guid companyId)
    {
        CompanyName = companyName;
        CompanyId = companyId;
    }

    public string CompanyName { get; set; }
    public Guid CompanyId { get; set; }
    public string? IdpAlias { get; set; }
}

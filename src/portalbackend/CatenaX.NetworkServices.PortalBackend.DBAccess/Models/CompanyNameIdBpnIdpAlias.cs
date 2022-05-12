namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyNameIdBpnIdpAlias
{
    public CompanyNameIdBpnIdpAlias(string companyName, Guid companyId)
    {
        CompanyName = companyName;
        CompanyId = companyId;
    }

    public string CompanyName { get; set; }
    public string? Bpn { get; set; }
    public Guid CompanyId { get; set; }
    public string? IdpAlias { get; set; }
}

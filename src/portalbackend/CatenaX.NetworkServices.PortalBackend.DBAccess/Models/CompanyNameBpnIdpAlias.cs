namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyNameBpnIdpAlias
{
    public CompanyNameBpnIdpAlias(Guid companyId, string companyName)
    {
        CompanyId = companyId;
        CompanyName = companyName;
    }

    public Guid CompanyId { get; }
    public string CompanyName { get; }
    public string? BusinessPartnerNumber  { get; set; }
    public string? IdpAlias { get; set; }
}

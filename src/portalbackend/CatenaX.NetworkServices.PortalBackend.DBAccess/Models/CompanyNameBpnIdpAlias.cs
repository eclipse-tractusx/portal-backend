namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyNameBpnIdpAlias
{
    public CompanyNameBpnIdpAlias(string companyName)
    {
        CompanyName = companyName;
    }

    public string CompanyName { get; set; }
    public string? Bpn  { get; set; }
    public string? IdpAlias { get; set; }
}

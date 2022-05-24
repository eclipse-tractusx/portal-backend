using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyServiceAccountWithClientId
{
    public CompanyServiceAccountWithClientId(CompanyServiceAccount companyServiceAccount, string clientId, string clientClientId)
    {
        CompanyServiceAccount = companyServiceAccount;
        ClientId = clientId;
        ClientClientId = clientClientId;
    }
    
    public CompanyServiceAccount CompanyServiceAccount { get; set; }
    public string ClientId;
    public string ClientClientId { get; set; }
}

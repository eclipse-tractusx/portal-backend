using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class ServiceAccountWithClientId
{
    public ServiceAccountWithClientId(CompanyServiceAccount companyServiceAccount, string clientClientId)
    {
        CompanyServiceAccount = companyServiceAccount;
        ClientClientId = clientClientId;
    }
    
    public CompanyServiceAccount CompanyServiceAccount { get; set; }
    public string ClientClientId { get; set; }
}

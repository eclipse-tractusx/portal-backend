using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyServiceAccountWithRoleDataClientId
{
    public CompanyServiceAccountWithRoleDataClientId(CompanyServiceAccount companyServiceAccount, string clientId, string clientClientId, IEnumerable<UserRoleData> userRoleDatas)
    {
        CompanyServiceAccount = companyServiceAccount;
        ClientId = clientId;
        ClientClientId = clientClientId;
        UserRoleDatas = userRoleDatas;
    }
    
    public CompanyServiceAccount CompanyServiceAccount { get; set; }
    public string ClientId;
    public string ClientClientId { get; set; }
    public IEnumerable<UserRoleData> UserRoleDatas { get; set; }
}

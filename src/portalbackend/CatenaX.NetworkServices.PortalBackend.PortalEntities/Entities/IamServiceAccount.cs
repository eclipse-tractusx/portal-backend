using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamServiceAccount
{
    private IamServiceAccount()
    {
        ClientId = null!;
        ClientClientId = null!;
        UserEntityId = null!;
    }

    public IamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId)
    {
        ClientId = clientId;
        ClientClientId = clientClientId;
        UserEntityId = userEntityId;
        CompanyServiceAccountId = companyServiceAccountId;
    }

    [Key]
    [StringLength(36)]
    public string ClientId { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    [StringLength(36)]
    public string UserEntityId { get; private set; }

    public Guid CompanyServiceAccountId { get; private set; }

    // Navigation properties
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; set; }
}

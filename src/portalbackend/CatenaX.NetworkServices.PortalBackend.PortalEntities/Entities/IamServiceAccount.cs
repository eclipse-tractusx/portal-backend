using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class IamServiceAccount
{
    private IamServiceAccount()
    {
        UserEntityId = null!;
        ClientClientId = null!;
    }

    public IamServiceAccount(string userEntityId, string clientClientId, Guid companyServiceAccountId)
    {
        UserEntityId = userEntityId;
        ClientClientId = clientClientId;
        CompanyServiceAccountId = companyServiceAccountId;
    }

    [Key]
    [StringLength(36)]
    public string UserEntityId { get; private set; }

    [StringLength(255)]
    public string ClientClientId { get; private set; }

    public Guid CompanyServiceAccountId { get; private set; }

    // Navigation properties
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; set; }
}

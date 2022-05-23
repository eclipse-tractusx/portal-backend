using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyServiceAccountStatus
{
    private CompanyServiceAccountStatus()
    {
        Label = null!;
        CompanyServiceAccounts = new HashSet<CompanyServiceAccount>();
    }

    public CompanyServiceAccountStatus(CompanyServiceAccountStatusId companyServiceAccountStatusId) : this()
    {
        Id = companyServiceAccountStatusId;
        Label = companyServiceAccountStatusId.ToString();
    }

    [Key]
    public CompanyServiceAccountStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<CompanyServiceAccount> CompanyServiceAccounts { get; private set; }
}

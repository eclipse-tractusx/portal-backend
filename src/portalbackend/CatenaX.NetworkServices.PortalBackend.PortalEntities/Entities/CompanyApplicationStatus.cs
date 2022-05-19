using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyApplicationStatus
{
    private CompanyApplicationStatus()
    {
        Label = null!;
        CompanyApplications = new HashSet<CompanyApplication>();
    }

    public CompanyApplicationStatus(CompanyApplicationStatusId applicationStatusId) : this()
    {
        ApplicationStatusId = applicationStatusId;
        Label = applicationStatusId.ToString();
    }

    public CompanyApplicationStatusId ApplicationStatusId { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<CompanyApplication> CompanyApplications { get; private set; }
}

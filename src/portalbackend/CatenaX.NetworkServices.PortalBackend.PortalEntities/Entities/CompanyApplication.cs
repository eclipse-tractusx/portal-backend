using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyApplication
{
    private CompanyApplication()
    {
        Invitations = new HashSet<Invitation>();
    }

    public CompanyApplication(Guid id, Guid companyId, CompanyApplicationStatusId applicationStatusId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        CompanyId = companyId;
        ApplicationStatusId = applicationStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    public CompanyApplicationStatusId ApplicationStatusId { get; set; }
    public Guid CompanyId { get; private set; }

    // Navigation properties
    public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
    public virtual Company? Company { get; private set; }
    public virtual ICollection<Invitation> Invitations { get; private set; }
}

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyAssignedUseCase
{
    private CompanyAssignedUseCase() {}

    public CompanyAssignedUseCase(Guid companyId, Guid useCaseId)
    {
        CompanyId = companyId;
        UseCaseId = useCaseId;
    }

    public Guid CompanyId { get; private set; }
    public Guid UseCaseId { get; private set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
    public virtual UseCase? UseCase { get; private set; }
}

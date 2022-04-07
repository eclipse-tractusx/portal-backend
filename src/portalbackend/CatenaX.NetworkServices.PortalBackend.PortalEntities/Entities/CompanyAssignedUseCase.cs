using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyAssignedUseCase
    {
        public CompanyAssignedUseCase() {}
        public CompanyAssignedUseCase(Company company, UseCase useCase)
        {
            Company = company;
            UseCase = useCase;
        }

        public Guid CompanyId { get; set; }
        public Guid UseCaseId { get; set; }

        public virtual Company Company { get; set; }
        public virtual UseCase UseCase { get; set; }
    }
}

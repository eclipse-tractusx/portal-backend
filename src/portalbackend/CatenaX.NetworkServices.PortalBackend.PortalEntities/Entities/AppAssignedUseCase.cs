using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppAssignedUseCase
    {
        private AppAssignedUseCase() {}

        public AppAssignedUseCase(Guid appId, Guid useCaseId)
        {
            AppId = appId;
            UseCaseId = useCaseId;
        }

        public Guid AppId { get; private set; }
        public Guid UseCaseId { get; private set; }

        // Navigation properties
        public virtual App? App { get; private set; }
        public virtual UseCase? UseCase { get; private set; }
    }
}

using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppAssignedUseCase
    {
        public AppAssignedUseCase() {}
        public AppAssignedUseCase(App app, UseCase useCase)
        {
            App = app;
            UseCase = useCase;
        }

        public Guid AppId { get; set; }
        public Guid UseCaseId { get; set; }

        public virtual App App { get; set; }
        public virtual UseCase UseCase { get; set; }
    }
}

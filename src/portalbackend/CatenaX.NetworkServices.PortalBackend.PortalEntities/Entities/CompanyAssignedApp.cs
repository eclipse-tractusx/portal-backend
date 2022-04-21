using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyAssignedApp
    {
        private CompanyAssignedApp() {}

        public CompanyAssignedApp(Guid appId, Guid companyId)
        {
            AppId = appId;
            CompanyId = companyId;
        }

        public Guid CompanyId { get; private set; }
        public Guid AppId { get; private set; }

        public virtual App? App { get; private set; }
        public virtual Company? Company { get; private set; }
    }
}

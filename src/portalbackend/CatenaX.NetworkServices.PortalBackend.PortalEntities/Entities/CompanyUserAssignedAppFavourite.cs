using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserAssignedAppFavourite
    {
        private CompanyUserAssignedAppFavourite() {}

        public CompanyUserAssignedAppFavourite(Guid appId, Guid companyUserId)
        {
            AppId = appId;
            CompanyUserId = companyUserId;
        }

        public Guid CompanyUserId { get; private set; }
        public Guid AppId { get; private set; }

        // Navigation properties
        public virtual CompanyUser? CompanyUser { get; private set; }
        public virtual App? App { get; private set; }
    }
}

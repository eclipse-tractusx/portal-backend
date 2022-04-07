using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserAssignedAppFavourite
    {
        public CompanyUserAssignedAppFavourite() {}
        public CompanyUserAssignedAppFavourite(App app, CompanyUser companyUser)
        {
            App = app;
            CompanyUser = companyUser;
        }

        public Guid CompanyUserId { get; set; }
        public Guid AppId { get; set; }

        public virtual CompanyUser CompanyUser { get; set; }
        public virtual App App { get; set; }
    }
}

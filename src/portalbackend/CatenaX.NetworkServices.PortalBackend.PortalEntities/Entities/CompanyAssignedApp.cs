using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyAssignedApp
    {
        public CompanyAssignedApp() {}
        public CompanyAssignedApp(App app, Company company)
        {
            App = app;
            Company = company;
        }

        public Guid CompanyId { get; set; }
        public Guid AppId { get; set; }

        public virtual App App { get; set; }
        public virtual Company Company { get; set; }
    }
}

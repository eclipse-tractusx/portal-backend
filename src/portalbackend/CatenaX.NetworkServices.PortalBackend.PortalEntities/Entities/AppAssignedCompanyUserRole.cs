using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppAssignedCompanyUserRole
    {
        public AppAssignedCompanyUserRole() {}
        public AppAssignedCompanyUserRole(App app, CompanyUserRole companyUserRole)
        {
            App = app;
            CompanyUserRole = companyUserRole;
        }

        public Guid AppId { get; set; }
        public Guid CompanyUserRoleId { get; set; }

        public virtual App App { get; set; }
        public virtual CompanyUserRole CompanyUserRole { get; set; }
    }
}

using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyAssignedRole
    {
        public CompanyAssignedRole() {}
        public CompanyAssignedRole(Company company, CompanyRole companyRole)
        {
            Company = company;
            CompanyRole = companyRole;
        }

        public Guid CompanyId { get; set; }
        public int CompanyRoleId { get; set; }

        public virtual Company Company { get; set; }
        public virtual CompanyRole CompanyRole { get; set; }
    }
}

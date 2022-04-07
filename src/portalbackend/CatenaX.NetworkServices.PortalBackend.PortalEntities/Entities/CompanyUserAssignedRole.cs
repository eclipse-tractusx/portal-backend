using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyUserAssignedRole
    {
        public CompanyUserAssignedRole() {}
        public CompanyUserAssignedRole(CompanyUser companyUser, CompanyUserRole userRole)
        {
            CompanyUser = companyUser;
            UserRole = userRole;
        }

        public Guid CompanyUserId { get; set; }
        public Guid UserRoleId { get; set; }

        public virtual CompanyUser CompanyUser { get; set; }
        public virtual CompanyUserRole UserRole { get; set; }
    }
}

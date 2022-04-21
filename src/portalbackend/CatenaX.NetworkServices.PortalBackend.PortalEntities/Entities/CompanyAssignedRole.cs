using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class CompanyAssignedRole
    {
        private CompanyAssignedRole() {}

        public CompanyAssignedRole(Guid companyId, int companyRoleId)
        {
            CompanyId = companyId;
            CompanyRoleId = companyRoleId;
        }

        public Guid CompanyId { get; private set; }
        public int CompanyRoleId { get; private set; }

        // Navigation properties
        public virtual Company? Company { get; private set; }
        public virtual CompanyRole? CompanyRole { get; private set; }
    }
}

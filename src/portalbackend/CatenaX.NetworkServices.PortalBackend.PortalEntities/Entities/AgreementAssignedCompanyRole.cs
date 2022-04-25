using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AgreementAssignedCompanyRole
    {
        private AgreementAssignedCompanyRole() {}

        public AgreementAssignedCompanyRole(Guid agreementId, int companyRoleId)
        {
            AgreementId = agreementId;
            CompanyRoleId = companyRoleId;
        }

        public Guid AgreementId { get; private set; }
        public int CompanyRoleId { get; private set; }

        // Navigation properties
        public virtual Agreement? Agreement { get; private set; }
        public virtual CompanyRole? CompanyRole { get; private set; }
    }
}

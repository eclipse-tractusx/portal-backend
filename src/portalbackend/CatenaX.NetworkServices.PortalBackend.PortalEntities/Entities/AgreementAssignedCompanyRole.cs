using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AgreementAssignedCompanyRole
    {
        private AgreementAssignedCompanyRole() {}

        public AgreementAssignedCompanyRole(Guid agreementId, CompanyRoleId companyRoleId)
        {
            AgreementId = agreementId;
            CompanyRoleId = companyRoleId;
        }

        public Guid AgreementId { get; private set; }
        public CompanyRoleId CompanyRoleId { get; private set; }

        // Navigation properties
        public virtual Agreement? Agreement { get; private set; }
        public virtual CompanyRole? CompanyRole { get; private set; }
    }
}

using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AgreementAssignedCompanyRole
    {
        public AgreementAssignedCompanyRole() {}
        public AgreementAssignedCompanyRole(Agreement agreement, CompanyRole companyRole)
        {
            Agreement = agreement;
            CompanyRole = companyRole;
        }

        public Guid AgreementId { get; set; }
        public int CompanyRoleId { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual CompanyRole CompanyRole { get; set; }
    }
}

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using System;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyRoleAgreementConsentData
    {
        public CompanyRoleAgreementConsentData(Guid companyUserId, Guid companyId, IEnumerable<CompanyAssignedRole> companyAssignedRoles, IEnumerable<Consent> consents)
        {
            CompanyUserId = companyUserId;
            CompanyId = CompanyId;
            CompanyAssignedRoles = companyAssignedRoles;
            Consents = consents;
        }
        public Guid CompanyUserId { get; }
        public Guid CompanyId { get; }
        public IEnumerable<CompanyAssignedRole> CompanyAssignedRoles { get; }
        public IEnumerable<Consent> Consents { get; }
    }
}

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class AgreementsAssignedCompanyRoleData
    {
        public AgreementsAssignedCompanyRoleData(CompanyRoleId companyRoleId, IEnumerable<Guid> agreementIds)
        {
            CompanyRoleId = companyRoleId;
            AgreementIds = agreementIds;
        }
        public CompanyRoleId CompanyRoleId  { get; }
        public IEnumerable<Guid> AgreementIds { get; }
    }
}

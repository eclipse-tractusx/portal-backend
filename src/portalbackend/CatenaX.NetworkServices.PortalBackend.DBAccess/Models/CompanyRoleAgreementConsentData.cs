using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyRoleAgreementConsentData
    {
        public CompanyRoleAgreementConsentData(Guid companyUserId, Guid companyId, CompanyApplication companyApplication, IEnumerable<CompanyAssignedRole> companyAssignedRoles, IEnumerable<Consent> consents)
        {
            CompanyUserId = companyUserId;
            CompanyId = companyId;
            CompanyApplication = companyApplication;
            CompanyAssignedRoles = companyAssignedRoles;
            Consents = consents;
        }
        public Guid CompanyUserId { get; }
        public Guid CompanyId { get; }
        public CompanyApplication CompanyApplication { get; }
        public IEnumerable<CompanyAssignedRole> CompanyAssignedRoles { get; }
        public IEnumerable<Consent> Consents { get; }
    }
}

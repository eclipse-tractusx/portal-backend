using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyRoleAgreementConsents
    {
        public CompanyRoleAgreementConsents(IEnumerable<CompanyRoleId> companyRoleIds, IEnumerable<AgreementConsentStatus> agreementConsentStatuses)
        {
            CompanyRoleIds = companyRoleIds;
            AgreementConsentStatuses = agreementConsentStatuses;
        }

        private CompanyRoleAgreementConsents()
        {
            CompanyRoleIds = default!;
            AgreementConsentStatuses = default!;
        }

        [JsonPropertyName("companyRoles")]
        public IEnumerable<CompanyRoleId> CompanyRoleIds { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementConsentStatus> AgreementConsentStatuses { get; set; }
    }

    public class AgreementConsentStatus
    {
        public AgreementConsentStatus(Guid agreementId, ConsentStatusId consentStatusId)
        {
            AgreementId = agreementId;
            ConsentStatusId = consentStatusId;
        }

        private AgreementConsentStatus() {}

        [JsonPropertyName("agreementId")]
        public Guid AgreementId { get; set; }

        [JsonPropertyName("consentStatus")]
        public ConsentStatusId ConsentStatusId { get; set; }
    }
}

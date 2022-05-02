using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class RoleAgreementConsentStatus
    {
        private RoleAgreementConsentStatus()
        {
            agreementConsentStatuses = default!;
        }

        [JsonPropertyName("companyRole")]
        public CompanyRoleId companyRoleId { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementConsentStatus> agreementConsentStatuses { get; set; }
    }

    public class AgreementConsentStatus
    {
        [JsonPropertyName("agreementId")]
        public Guid agreementId { get; set; }

        [JsonPropertyName("consentStatus")]
        public ConsentStatusId consentStatusId { get; set; }
    }
}
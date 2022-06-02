using System.Text.Json.Serialization;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class RegistrationData
    {
        public RegistrationData(Guid companyId, string name, IEnumerable<CompanyRoleId> companyRoleIds, IEnumerable<RegistrationDocumentNames> documents, IEnumerable<AgreementConsentStatusForRegistrationData> agreementConsentStatuses)
        {
            CompanyId = companyId;
            Name = name;
            CompanyRoleIds = companyRoleIds;
            Documents = documents;
            AgreementConsentStatuses = agreementConsentStatuses;
        }

        [JsonPropertyName("companyId")]
        public Guid CompanyId { get; set; }

        [JsonPropertyName("bpn")]
        public string? BusinessPartnerNumber { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("shortName")]
        public string? Shortname { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("streetAdditional")]
        public string? Streetadditional { get; set; }

        [JsonPropertyName("streetName")]
        public string? Streetname { get; set; }

        [JsonPropertyName("streetNumber")]
        public string? Streetnumber { get; set; }

        [JsonPropertyName("zipCode")]
        public decimal? Zipcode { get; set; }

        [JsonPropertyName("countryAlpha2Code")]
        public string? CountryAlpha2Code { get; set; }

        [JsonPropertyName("countryDe")]
        public string? CountryDe { get; set; }

        [JsonPropertyName("taxId")]
        public string? TaxId { get; set; }

        [JsonPropertyName("companyRoles")]
        public IEnumerable<CompanyRoleId> CompanyRoleIds { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementConsentStatusForRegistrationData> AgreementConsentStatuses { get; set; }
        public IEnumerable<RegistrationDocumentNames> Documents { get; set; }

    }

    public class AgreementConsentStatusForRegistrationData
    {
        public AgreementConsentStatusForRegistrationData(Guid agreementId, ConsentStatusId consentStatusId)
        {
            AgreementId = agreementId;
            ConsentStatusId = consentStatusId;
        }

        private AgreementConsentStatusForRegistrationData() {}

        [JsonPropertyName("agreementId")]
        public Guid AgreementId { get; set; }

        [JsonPropertyName("consentStatus")]
        public ConsentStatusId ConsentStatusId { get; set; }
    }

}

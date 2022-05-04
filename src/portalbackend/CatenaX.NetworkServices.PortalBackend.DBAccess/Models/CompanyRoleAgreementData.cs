using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyRoleAgreementData
    {
        public CompanyRoleAgreementData(IEnumerable<CompanyRoleData> companyRoleData, IEnumerable<AgreementData> agreementData)
        {
            CompanyRoleData = companyRoleData;
            AgreementData = agreementData;
        }

        [JsonPropertyName("companyRoles")]
        public IEnumerable<CompanyRoleData> CompanyRoleData { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementData> AgreementData { get; set; }
    }
    public class CompanyRoleData
    {
        public CompanyRoleData(CompanyRoleId companyRoleId, IDictionary<string, string> companyRoleDescriptions, IEnumerable<Guid> agreementIds)
        {
            CompanyRoleId = companyRoleId;
            CompanyRoleDescriptions = companyRoleDescriptions;
            AgreementIds = agreementIds;
        }

        [JsonPropertyName("companyRole")]
        public CompanyRoleId CompanyRoleId { get; set; }

        [JsonPropertyName("descriptions")]
        public IDictionary<string,string> CompanyRoleDescriptions { get; set; }

        [JsonPropertyName("agreementIds")]
        public IEnumerable<Guid> AgreementIds { get; set; }
    }

    public class AgreementData
    {
        public AgreementData(Guid agreementId, string agreementName)
        {
            AgreementId = agreementId;
            AgreementName = agreementName;
        }

        [JsonPropertyName("agreementId")]
        public Guid AgreementId { get; set; }

        [JsonPropertyName("name")]
        public string AgreementName { get; set; }
    }
}

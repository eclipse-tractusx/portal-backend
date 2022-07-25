using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
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
}

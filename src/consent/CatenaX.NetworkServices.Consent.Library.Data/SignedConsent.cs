using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Cosent.Library.Data
{
    public class SignedConsent
    {
        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }

        [JsonPropertyName("signature_date")]
        public DateTime SignatureDate { get; set; }

        [JsonPropertyName("signatory")]
        public string Signatory { get; set; }

        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        [JsonPropertyName("role_title")]
        public string RoleTitle { get; set; }

        [JsonPropertyName("consent_id")]
        public int ConsentId { get; set; }

        [JsonPropertyName("consent_title")]
        public string ConsentTitle { get; set; }
    }
}

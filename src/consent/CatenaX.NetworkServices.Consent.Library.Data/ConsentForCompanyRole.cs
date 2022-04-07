using System;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Cosent.Library.Data
{
    public class ConsentForCompanyRole
    {
        [JsonPropertyName("role_id")]
        public int Id { get; set; }

        [JsonPropertyName("role_title")]
        public string Title { get; set; }

        [JsonPropertyName("consent_id")]
        public int ConsentId {  get; set; }

        [JsonPropertyName("consent_title")]
        public string ConstentTitle {  get; set; }

        [JsonPropertyName("link")]
        public string Link {  get; set; }
    }
}

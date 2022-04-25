using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Consent.Library.Data
{
    public class Consent
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }
    }
}

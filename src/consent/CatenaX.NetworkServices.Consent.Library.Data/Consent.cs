using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Cosent.Library.Data
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

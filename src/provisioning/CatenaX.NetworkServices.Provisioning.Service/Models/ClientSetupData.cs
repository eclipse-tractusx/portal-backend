using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Provisioning.Service.Models
{
    public class ClientSetupData
    {
        [JsonPropertyName("redirectUrl")]
        public string redirectUrl { get; set; }
    }
}

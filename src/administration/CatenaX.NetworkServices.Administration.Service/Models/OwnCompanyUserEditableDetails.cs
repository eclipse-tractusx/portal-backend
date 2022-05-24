using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class OwnCompanyUserEditableDetails
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

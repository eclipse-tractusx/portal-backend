using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Framework.ErrorHandling;

public class ErrorResponse
{
    public ErrorResponse(string type, string title, int status, IDictionary<string,IEnumerable<string>> errors)
    {
        Type = type;
        Title = title;
        Status = status;
        Errors = errors;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("errors")]
    public IDictionary<string,IEnumerable<string>> Errors { get; set; }
}

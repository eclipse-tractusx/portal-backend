using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// model to specify Message
/// </summary>
public class Message
{
    
    /// <summary>
    /// Name of the Role
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Message Description
    /// </summary>
    [JsonPropertyName("info")]
    public MessageDetail Info { get; set; }
}

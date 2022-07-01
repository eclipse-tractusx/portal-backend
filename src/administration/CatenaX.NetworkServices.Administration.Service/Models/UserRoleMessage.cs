using System.Collections;
using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// model to specify Message for Adding User Role
/// </summary>
public class UserRoleMessage
{
    
    /// <summary>
    /// Success Message
    /// </summary>
    [JsonPropertyName("success")]
    public IEnumerable<Message> Success { get; set; }

    /// <summary>
    /// Warning Message
    /// </summary>
    [JsonPropertyName("warning")]
    public IEnumerable<Message> Warning { get; set; }
}

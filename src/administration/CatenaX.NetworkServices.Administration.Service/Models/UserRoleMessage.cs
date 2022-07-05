using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// model to specify Message for Adding User Role
/// </summary>
public class UserRoleMessage
{
    public UserRoleMessage(IEnumerable<Message> success, IEnumerable<Message> warning)
    {
        Success = success;
        Warning = warning;
    }
    
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

    /// <summary>
    /// model to specify Message
    /// </summary>
    public class Message
    {
        public Message(string name, Detail info)
        {
            Name = name;
            Info = info;
        }

        /// <summary>
        /// Name of the Role
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Message Description
        /// </summary>
        [JsonPropertyName("info")]
        public Detail Info { get; set; }
    }

    public enum Detail
    {
        ROLE_DOESNT_EXIST,
        ROLE_ADDED,
        ROLE_ALREADY_ADDED
    }
}

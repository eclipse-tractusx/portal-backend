using System.Collections;
using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// model to specify Role Information for User.
/// </summary>
public class UserRoleInfo
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="companyUserId"> CompanyUser Id</param>
    /// <param name="userEntityId">UserEntity Id</param>
    /// <param name="roles">Role of the User</param>
    public UserRoleInfo(Guid companyUserId, string userEntityId, IEnumerable<string> roles)
    {
        this.CompanyUserId = companyUserId;
        this.UserEntityId = userEntityId;
        this.Roles = roles;
    }

    /// <summary>
    /// CompanyUser Id
    /// </summary>
    /// <value></value>
    [JsonPropertyName("companyUserId")]
    public Guid CompanyUserId { get; set; }

    /// <summary>
    /// UserEntity Id
    /// </summary>
    /// <value></value>
    [JsonPropertyName("userEntityId")]
    public string UserEntityId { get; set; }

    /// <summary>
    /// Role of User
    /// </summary>
    /// <value></value>
    [JsonPropertyName("roles")]
    public IEnumerable<string> Roles { get; set; }
}

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
    /// <param name="roles">Role of the User</param>
    public UserRoleInfo(Guid companyUserId, IEnumerable<string> roles)
    {
        this.CompanyUserId = companyUserId;
        this.Roles = roles;
    }

    /// <summary>
    /// CompanyUser Id
    /// </summary>
    [JsonPropertyName("companyUserId")]
    public Guid CompanyUserId { get; set; }

    /// <summary>
    /// Role of User
    /// </summary>
    [JsonPropertyName("roles")]
    public IEnumerable<string> Roles { get; set; }
}

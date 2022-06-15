using System.Collections;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
/// <summary>
/// Model for Company and Iam User
/// </summary>
public class CompanyIamUser
{
    /// <summary>
    /// Target IamUser Id
    /// </summary>
    /// <value></value>
    public string? TargetIamUserId { get; set; }

    /// <summary>
    /// Idp Name
    /// </summary>
    /// <value></value>
    public string? IdpName { get; set; }

    /// <summary>
    /// Role Ids of User
    /// </summary>
    /// <value></value>
    public IEnumerable<Guid> RoleIds { get; set; }
}


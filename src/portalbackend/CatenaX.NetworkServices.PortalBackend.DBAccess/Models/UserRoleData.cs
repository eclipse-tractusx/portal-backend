using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Basic model for user role data needed to display user roles.
/// </summary>
public record UserRoleData(
        [property: JsonPropertyName("roleId")] Guid UserRoleId,
        [property: JsonPropertyName("clientId")] string ClientClientId,
        [property: JsonPropertyName("roleName")] string UserRoleText);
/// <summary>
/// Basic model for user role data needed to display user roles with description.
/// </summary>
public record UserRoleWithDescription(
        [property: JsonPropertyName("roleId")] Guid UserRoleId,
        [property: JsonPropertyName("roleName")] string UserRoleText,
        [property: JsonPropertyName("roleDescription")] string RoleDescription);

using System.Text.Json.Serialization;
using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// Creation Information for the service account
/// </summary>
/// <param name="Name">Name of the service account</param>
/// <param name="Description">Description for the service account table</param>
/// <param name="IamClientAuthMethod">Method of the authentication</param>
/// <param name="UserRoleIds">ids for the user roles</param>
public record ServiceAccountCreationInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("authenticationType")] IamClientAuthMethod IamClientAuthMethod,
    [property: JsonPropertyName("roleIds")] IEnumerable<Guid> UserRoleIds);

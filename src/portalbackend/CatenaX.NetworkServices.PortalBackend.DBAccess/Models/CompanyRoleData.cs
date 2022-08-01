using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    /// <summary>
    /// Basic model for company role agreements data
    /// </summary>
    public record CompanyRoleData(
        [property: JsonPropertyName("companyRole")] CompanyRoleId CompanyRoleId,
        [property: JsonPropertyName("descriptions")] IDictionary<string, string> CompanyRoleDescriptions,
        [property: JsonPropertyName("agreementIds")] IEnumerable<Guid> AgreementIds);

    /// <summary>
    /// Basic model for company role data needed to display company roles with description.
    /// </summary>
    public record CompanyRolesDetails(
        [property: JsonPropertyName("companyRole")] string CompanyRole,
        [property: JsonPropertyName("roleDescription")] string RoleDescription);
}

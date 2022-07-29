using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    /// <summary>
    /// Basic model for company role agreements data
    /// </summary>
    public record CompanyRoleData([property: JsonPropertyName("companyRole")] CompanyRoleId companyRoleId, [property: JsonPropertyName("descriptions")] IDictionary<string, string> companyRoleDescriptions, [property: JsonPropertyName("agreementIds")] IEnumerable<Guid> agreementIds);

    /// <summary>
    /// Basic model for company role data needed to display company roles with description.
    /// </summary>
    public record CompanyRolesDetails([property: JsonPropertyName("companyRole")] string companyRole, [property: JsonPropertyName("roleDescription")] string roleDescription);
}

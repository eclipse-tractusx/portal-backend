using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public record CompanyRoleAgreementData(
        [property: JsonPropertyName("companyRoles")] IEnumerable<CompanyRoleData> companyRoleData,
        [property: JsonPropertyName("agreements")] IEnumerable<AgreementData> agreementData);

}
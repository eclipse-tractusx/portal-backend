using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class CompanyRoleAgreementData
    {
        public CompanyRoleAgreementData(IEnumerable<CompanyRoleData> companyRoleData, IEnumerable<AgreementData> agreementData)
        {
            CompanyRoleData = companyRoleData;
            AgreementData = agreementData;
        }

        [JsonPropertyName("companyRoles")]
        public IEnumerable<CompanyRoleData> CompanyRoleData { get; set; }

        [JsonPropertyName("agreements")]
        public IEnumerable<AgreementData> AgreementData { get; set; }
    }
}

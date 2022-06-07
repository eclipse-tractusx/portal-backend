using System.Collections;
using System.Text.Json.Serialization;
namespace CatenaX.NetworkServices.Administration.Service.Models
{
    public class UserRoleInfo
    {
        public UserRoleInfo(Guid companyUserId, string userEntityId, IEnumerable<string> roles)
        {
            this.CompanyUserId = companyUserId;
            this.UserEntityId = userEntityId;
            this.Roles = roles;
        }
        [JsonPropertyName("companyUserId")]
        public Guid CompanyUserId { get; set; }
        [JsonPropertyName("userEntityId")]
        public string UserEntityId { get; set; }
        [JsonPropertyName("roles")]
        public IEnumerable<string> Roles { get; set; }
    }
}
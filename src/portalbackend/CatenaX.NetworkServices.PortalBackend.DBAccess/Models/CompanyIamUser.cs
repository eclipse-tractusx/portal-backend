using System.Collections;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyIamUser
    {
        public string? TargetIamUserId { get; set; }
        public Guid TargetCompanyUserId { get; set; }
        public string? IdpName { get; set; }
        public IEnumerable<Guid> RoleIds { get; set; }
    }
}

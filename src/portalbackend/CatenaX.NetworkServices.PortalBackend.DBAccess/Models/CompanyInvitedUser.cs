using System.Collections;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyInvitedUser
    {
        public CompanyInvitedUser(Guid companyUserId, string userEntityId, IEnumerable<Guid> roleIds)
        {
            CompanyUserId = companyUserId;
            UserEntityId = userEntityId;
            RoleIds = roleIds;
        }

        public Guid CompanyUserId { get; set; }
        public string UserEntityId { get; set; }
        public IEnumerable<Guid> RoleIds { get; set; }
    }

}

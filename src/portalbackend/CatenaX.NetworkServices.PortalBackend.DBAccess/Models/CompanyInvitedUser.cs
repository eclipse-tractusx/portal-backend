namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyInvitedUser
{
    public CompanyInvitedUser(Guid companyUserId, string userEntityId)
    {
        CompanyUserId = companyUserId;
        UserEntityId = userEntityId;
    }

    public Guid CompanyUserId { get; set; }
    public string UserEntityId { get; set; }
}

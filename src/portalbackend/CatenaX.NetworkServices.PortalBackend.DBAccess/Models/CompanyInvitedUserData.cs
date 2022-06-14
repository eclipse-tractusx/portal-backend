namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyInvitedUserData
{
    public CompanyInvitedUserData(Guid companyUserId, string userEntityId, IEnumerable<string> businessPartnerNumberss, IEnumerable<Guid> roleIds)
    {
        CompanyUserId = companyUserId;
        UserEntityId = userEntityId;
        BusinessPartnerNumbers = businessPartnerNumberss;
        RoleIds = roleIds;
    }

    public Guid CompanyUserId { get; set; }
    public string UserEntityId { get; set; }
    public IEnumerable<string> BusinessPartnerNumbers;
    public IEnumerable<Guid> RoleIds;

}

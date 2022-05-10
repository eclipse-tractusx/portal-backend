namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class UserRoleWithId
{
    public UserRoleWithId(string companyUserRoleText, Guid companyUserRoleId)
    {
        CompanyUserRoleText = companyUserRoleText;
        CompanyUserRoleId = companyUserRoleId;
    }
    public string CompanyUserRoleText { get; set; }
    public Guid CompanyUserRoleId { get; set; }
}

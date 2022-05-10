namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserRoleWithId
{
    public CompanyUserRoleWithId(string companyUserRoleText, Guid companyUserRoleId)
    {
        CompanyUserRoleText = companyUserRoleText;
        CompanyUserRoleId = companyUserRoleId;
    }
    public string CompanyUserRoleText { get; set; }
    public Guid CompanyUserRoleId { get; set; }
}

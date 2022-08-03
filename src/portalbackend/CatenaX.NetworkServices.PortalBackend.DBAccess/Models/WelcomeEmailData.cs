namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class WelcomeEmailData
{
    public WelcomeEmailData(Guid companyUserId, string? firstName, string? lastName, string? email, string companyName, IEnumerable<Guid> roleIds)
    {
        CompanyUserId = companyUserId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CompanyName= companyName;
        RoleIds = roleIds;
    }

    public Guid CompanyUserId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string CompanyName { get; set; }

    public IEnumerable<Guid> RoleIds { get; set; }

}

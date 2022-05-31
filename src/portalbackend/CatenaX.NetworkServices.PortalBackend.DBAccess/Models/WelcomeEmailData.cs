namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class WelcomeEmailData
{
    public WelcomeEmailData(string? firstName, string? lastName, string? email, string companyName)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        CompanyName= companyName;
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string CompanyName { get; set; }
}

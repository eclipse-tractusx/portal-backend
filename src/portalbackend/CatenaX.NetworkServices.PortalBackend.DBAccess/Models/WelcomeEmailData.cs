namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public record WelcomeEmailData(Guid CompanyUserId, string? FirstName, string? LastName, string? Email, string CompanyName);

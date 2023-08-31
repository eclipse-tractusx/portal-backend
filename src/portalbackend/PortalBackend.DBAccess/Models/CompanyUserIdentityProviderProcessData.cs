namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record CompanyUserIdentityProviderProcessData(
    Guid CompanyUserId,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    string UserId,
    string CompanyName,
    string? Bpn,
    string Alias,
    string ProviderUserId
);

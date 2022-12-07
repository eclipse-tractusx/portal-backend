namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

public record CompanyNameIdpAliasData(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber, Guid CompanyUserId, string IdpAlias, bool IsSharedIdp);

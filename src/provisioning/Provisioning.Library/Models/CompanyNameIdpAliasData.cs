namespace Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;

public record CompanyNameIdpAliasData(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber, Guid companyUserId, string IdpAlias, bool IsShardIdp);

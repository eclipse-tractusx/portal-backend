using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Tests.Shared.Data;

public static class Entities
{
    public static CompanyApplication WithCompanyApplication(Guid id, Guid companyId,
        CompanyApplicationStatusId companyApplicationStatusId) =>
        new(id, companyId, companyApplicationStatusId, DateTimeOffset.UtcNow);
}
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public class CompanyApplicationStatusUserData
{
    public CompanyApplicationStatusUserData(CompanyApplicationStatusId companyApplicationStatusId)
    {
        CompanyApplicationStatusId = companyApplicationStatusId;
    }
    public CompanyApplicationStatusId CompanyApplicationStatusId { get; }
    public Guid CompanyUserId { get; set; }
}

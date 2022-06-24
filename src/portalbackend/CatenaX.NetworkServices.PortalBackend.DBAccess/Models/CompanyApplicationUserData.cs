using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyApplicationUserData
{
    public CompanyApplicationUserData(CompanyApplication companyApplication)
    {
        CompanyApplication = companyApplication;
    }

    public CompanyApplication CompanyApplication { get; }
    public Guid CompanyUserId { get; set; }
}

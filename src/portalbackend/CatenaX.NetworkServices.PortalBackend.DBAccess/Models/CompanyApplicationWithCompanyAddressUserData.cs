using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyApplicationWithCompanyAddressUserData
{
    public CompanyApplicationWithCompanyAddressUserData(CompanyApplication companyApplication)
    {
        CompanyApplication = companyApplication;
    }
    public CompanyApplication CompanyApplication { get; }
    public Guid CompanyUserId { get; set; }
}

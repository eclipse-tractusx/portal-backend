using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserWithIdpData
{
    public CompanyUserWithIdpData(CompanyUser companyUser, string iamIdpAlias)
    {
        CompanyUser = companyUser;
        IamIdpAlias = iamIdpAlias;
    }

    public CompanyUser CompanyUser { get; }
    public string IamIdpAlias { get; }
}

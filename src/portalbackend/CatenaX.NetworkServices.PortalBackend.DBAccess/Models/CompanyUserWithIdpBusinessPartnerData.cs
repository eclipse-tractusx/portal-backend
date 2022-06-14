using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserWithIdpBusinessPartnerData
{
    public CompanyUserWithIdpBusinessPartnerData(CompanyUser companyUser, string iamIdpAlias, IEnumerable<string> businessPartnerNumbers)
    {
        CompanyUser = companyUser;
        IamIdpAlias = iamIdpAlias;
        BusinessPartnerNumbers = businessPartnerNumbers;
    }

    public CompanyUser CompanyUser { get; }
    public string IamIdpAlias { get; }
    public IEnumerable<string> BusinessPartnerNumbers;
}

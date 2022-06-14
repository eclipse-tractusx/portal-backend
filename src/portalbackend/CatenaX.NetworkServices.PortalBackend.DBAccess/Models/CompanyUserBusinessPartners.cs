namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

public class CompanyUserBusinessPartners
{
    public CompanyUserBusinessPartners(string userEntityId, IEnumerable<string> assignedBusinessPartnerNumbers)
    {
        UserEntityId = userEntityId;
        AssignedBusinessPartnerNumbers = assignedBusinessPartnerNumbers;
    }

    public string UserEntityId { get; }
    public IEnumerable<string> AssignedBusinessPartnerNumbers { get; }
}

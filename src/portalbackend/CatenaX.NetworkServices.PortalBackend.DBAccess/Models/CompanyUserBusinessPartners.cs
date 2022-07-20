using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
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

public class CompanyUserBusinessPartnerNumbersDetails
{
    public CompanyUserBusinessPartnerNumbersDetails(string userEntityId, CompanyUserAssignedBusinessPartner? assignedBusinessPartnerNumbers, bool isValidUser)
    {
        UserEntityId = userEntityId;
        AssignedBusinessPartnerNumbers = assignedBusinessPartnerNumbers;
        IsValidUser = isValidUser;
    }

    public string UserEntityId { get; set;}
    public CompanyUserAssignedBusinessPartner? AssignedBusinessPartnerNumbers { get; set; }
    public bool IsValidUser { get; set;}
}

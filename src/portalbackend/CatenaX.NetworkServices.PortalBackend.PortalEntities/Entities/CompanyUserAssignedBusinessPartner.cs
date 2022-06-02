using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserAssignedBusinessPartner
{
    public CompanyUserAssignedBusinessPartner(string businessPartnerNumber, Guid companyUserId)
    {
        BusinessPartnerNumber = businessPartnerNumber;
        CompanyUserId = companyUserId;
    }

    [MaxLength(20)]
    public string BusinessPartnerNumber;

    public Guid CompanyUserId;

    // Navigation properties
    public virtual BusinessPartner? BusinessPartner { get; set; }
    public virtual CompanyUser? CompanyUser { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserAssignedBusinessPartner
{
    public CompanyUserAssignedBusinessPartner(Guid companyUserId, string businessPartnerNumber)
    {
        CompanyUserId = companyUserId;
        BusinessPartnerNumber = businessPartnerNumber;
    }

    public Guid CompanyUserId;

    [MaxLength(20)]
    public string BusinessPartnerNumber;

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class BusinessPartner
{
    private BusinessPartner()
    {
        BusinessPartnerNumber = null!;
        ChildBusinessPartners = new HashSet<BusinessPartner>();
        CompanyUsers = new HashSet<CompanyUser>();
    }

    public BusinessPartner(string businessPartnerNumber, DateTimeOffset dateCreated) : this()
    {
        BusinessPartnerNumber = businessPartnerNumber;
        DateCreated = dateCreated;
    }

    [MaxLength(20)]
    public string BusinessPartnerNumber { get; set; }

    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(20)]
    public string? ParentBusinessPartnerNumber { get; set; }

    // Navigation properties
    public virtual Company? Company { get; set; }
    public virtual BusinessPartner? ParentBusinessPartner { get; set; }
    public virtual ICollection<BusinessPartner> ChildBusinessPartners { get; private set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
}

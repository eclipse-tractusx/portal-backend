using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Company
{
    private Company()
    {
        Name = null!;
        Agreements = new HashSet<Agreement>();
        BoughtApps = new HashSet<App>();
        ProvidedApps = new HashSet<App>();
        CompanyApplications = new HashSet<CompanyApplication>();
        CompanyAssignedRoles = new HashSet<CompanyAssignedRole>();
        IdentityProviders = new HashSet<IdentityProvider>();
        CompanyUsers = new HashSet<CompanyUser>();
        CompanyServiceAccounts = new HashSet<CompanyServiceAccount>();
        Consents = new HashSet<Consent>();
        CompanyRoles = new HashSet<CompanyRole>();
        UseCases = new HashSet<UseCase>();
        ProvidedConnectors = new HashSet<Connector>();
        HostedConnectors = new HashSet<Connector>();
    }
    
    public Company(Guid id, string name, CompanyStatusId companyStatusId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        Name = name;
        CompanyStatusId = companyStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(20)]
    public string? Bpn { get; set; }

    [MaxLength(20)]
    public string? TaxId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Parent { get; set; }

    [MaxLength(255)]
    public string? Shortname { get; set; }

    public CompanyStatusId CompanyStatusId { get; set; }

    public Guid? AddressId { get; set; }

    // Navigation properties
    public virtual Address? Address { get; set; }
    public virtual CompanyStatus? CompanyStatus { get; set; }
    public virtual ICollection<App> ProvidedApps { get; private set; }
    public virtual ICollection<App> BoughtApps { get; private set; }
    public virtual ICollection<Agreement> Agreements { get; private set; }
    public virtual ICollection<CompanyApplication> CompanyApplications { get; private set; }
    public virtual ICollection<IdentityProvider> IdentityProviders { get; private set; }
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
    public virtual ICollection<CompanyServiceAccount> CompanyServiceAccounts { get; private set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<CompanyRole> CompanyRoles { get; private set; }
    public virtual ICollection<CompanyAssignedRole> CompanyAssignedRoles { get; private set; }
    public virtual ICollection<UseCase> UseCases { get; private set; }
    public virtual ICollection<Connector> ProvidedConnectors { get; private set; }
    public virtual ICollection<Connector> HostedConnectors { get; private set; }
}

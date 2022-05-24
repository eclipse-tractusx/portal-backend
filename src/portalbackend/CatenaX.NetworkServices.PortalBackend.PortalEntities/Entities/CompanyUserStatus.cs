using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyUserStatus
{
    private CompanyUserStatus()
    {
        Label = null!;
        CompanyUsers = new HashSet<CompanyUser>();
    }

    public CompanyUserStatus(CompanyUserStatusId companyUserStatusId) : this()
    {
        Id = companyUserStatusId;
        Label = companyUserStatusId.ToString();
    }

    public CompanyUserStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<CompanyUser> CompanyUsers { get; private set; }
}

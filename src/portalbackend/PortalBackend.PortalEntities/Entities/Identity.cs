using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditIdentity20230526))]
public class Identity : IBaseEntity, IAuditableV1
{
    public Identity(Guid id, DateTimeOffset dateCreated, Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId)
    {
        Id = id;
        DateCreated = dateCreated;
        CompanyId = companyId;
        UserStatusId = userStatusId;
        IdentityTypeId = identityTypeId;

        IdentityAssignedRoles = new HashSet<IdentityAssignedRole>();
        CreatedNotifications = new HashSet<Notification>();
    }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    public Guid CompanyId { get; set; }

    [JsonPropertyName("user_status_id")]
    public UserStatusId UserStatusId { get; set; }

    [StringLength(36)]
    public string? UserEntityId { get; set; }

    public IdentityTypeId IdentityTypeId { get; set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    [AuditLastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; set; }
    public virtual CompanyServiceAccount? CompanyServiceAccount { get; set; }
    public virtual Company? Company { get; set; }
    public virtual IdentityUserStatus? IdentityStatus { get; set; }
    public virtual IdentityType? IdentityType { get; set; }
    public virtual ICollection<Notification> CreatedNotifications { get; private set; }
    public virtual ICollection<IdentityAssignedRole> IdentityAssignedRoles { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
}

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class MailingStatus
{
    private MailingStatus()
    {
        Label = null!;
        MailingInformations = new HashSet<MailingInformation>();
    }

    public MailingStatus(MailingStatusId mailingStatusId) : this()
    {
        Id = mailingStatusId;
        Label = mailingStatusId.ToString();
    }

    public MailingStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<MailingInformation> MailingInformations { get; private set; }
}

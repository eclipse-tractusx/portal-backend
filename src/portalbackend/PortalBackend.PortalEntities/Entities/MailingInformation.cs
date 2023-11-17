using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class MailingInformation : IBaseEntity
{
    private MailingInformation()
    {
        Email = null!;
        Template = null!;
        MailParameter = null!;
    }

    public MailingInformation(Guid id, Guid processId, string email, string template, Dictionary<string, string> mailParameter, MailingStatusId mailingStatusId)
        : this()
    {
        Id = id;
        ProcessId = processId;
        Email = email;
        Template = template;
        MailParameter = mailParameter;
        MailingStatusId = mailingStatusId;
    }

    public Guid Id { get; }

    public Guid ProcessId { get; set; }

    public string Email { get; set; }

    public string Template { get; set; }

    public MailingStatusId MailingStatusId { get; set; }

    public Dictionary<string, string> MailParameter { get; set; }

    public virtual Process? Process { get; private set; }

    public virtual MailingStatus? MailingStatus { get; private set; }
}

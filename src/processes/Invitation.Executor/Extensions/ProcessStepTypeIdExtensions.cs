using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Extensions;

public static class ProcessStepTypeIdExtensions
{
    public static IEnumerable<ProcessStepTypeId>? GetRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.INVITATION_SETUP_IDP => new[] { ProcessStepTypeId.RETRIGGER_INVITATION_SETUP_IDP },
            ProcessStepTypeId.INVITATION_CREATE_USER => new[] { ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER },
            ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP => new[] { ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP },
            ProcessStepTypeId.INVITATION_SEND_MAIL => new[] { ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL },
            _ => null
        };

}

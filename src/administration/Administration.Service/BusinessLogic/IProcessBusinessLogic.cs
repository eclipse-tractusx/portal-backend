using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IProcessBusinessLogic
{
    /// <summary>
    /// Retriggers the given process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    /// <param name="stepToTrigger">The step to retrigger</param>
    Task TriggerProcessStep(Guid offerSubscriptionId, ProcessStepTypeId stepToTrigger);
}
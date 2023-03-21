using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;

public interface IOfferProviderBusinessLogic
{
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, IEnumerable<ProcessStepTypeId>? stepsToSkip, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProvider(Guid offerSubscriptionId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, IEnumerable<ProcessStepTypeId>? stepsToSkip, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProviderCallback(Guid offerSubscriptionId, CancellationToken cancellationToken);
}

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

public class OfferSubscriptionProcessService : IOfferSubscriptionProcessService
{
    private readonly IPortalRepositories _portalRepositories;

    public OfferSubscriptionProcessService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    async Task<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData> IOfferSubscriptionProcessService.VerifySubscriptionAndProcessSteps(Guid offerSubscriptionId, ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId>? processStepTypeIds)
    {
        var allProcessStepTypeIds = processStepTypeIds == null
            ? new[] { processStepTypeId }
            : processStepTypeIds.Append(processStepTypeId);

        var processData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetProcessStepData(offerSubscriptionId, allProcessStepTypeIds).ConfigureAwait(false);

        processData.ValidateOfferSubscriptionProcessData(offerSubscriptionId, new[] { ProcessStepStatusId.TODO });
        var processStep = processData!.ProcessSteps!.SingleOrDefault(step => step.ProcessStepTypeId == processStepTypeId);
        if (processStep is null)
        {
            throw new ConflictException($"offer subscription {offerSubscriptionId} process step {processStepTypeId} is not eligible to run");
        }
        return processData.CreateManualOfferSubscriptionProcessStepData(offerSubscriptionId, processStep);
    }

    public void FinalizeChecklistEntryAndProcessSteps(IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData context, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.AttachAndModifyProcessStep(context.ProcessStepId, null, step => step.ProcessStepStatusId = ProcessStepStatusId.DONE);
        if (nextProcessStepTypeIds == null || !nextProcessStepTypeIds.Any())
        {
            return;
        }

        processStepRepository.CreateProcessStepRange(
            nextProcessStepTypeIds
                .Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId))
                .Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, context.Process.Id)));

        if (context.Process.ReleaseLock())
            return;

        _portalRepositories.Attach(context.Process);
        context.Process.UpdateVersion();
    }
}

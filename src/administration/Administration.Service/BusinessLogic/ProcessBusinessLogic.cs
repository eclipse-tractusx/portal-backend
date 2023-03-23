using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class ProcessBusinessLogic : IProcessBusinessLogic
{
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IPortalRepositories _portalRepositories;

    public ProcessBusinessLogic(IOfferSubscriptionProcessService offerSubscriptionProcessService, IPortalRepositories portalRepositories)
    {
        _offerSubscriptionProcessService = offerSubscriptionProcessService;
        _portalRepositories = portalRepositories;
    }
    
    /// <inheritdoc />
    public async Task TriggerProcessStep(Guid offerSubscriptionId, ProcessStepTypeId stepToTrigger)
    {
        var context = await _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId, stepToTrigger, null)
            .ConfigureAwait(false);
        
        _offerSubscriptionProcessService.FinalizeProcessSteps(context, Enumerable.Repeat(stepToTrigger.GetStepToRetrigger(), 1));
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
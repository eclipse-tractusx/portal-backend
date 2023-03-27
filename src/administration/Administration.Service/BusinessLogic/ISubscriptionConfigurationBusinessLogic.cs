using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface ISubscriptionConfigurationBusinessLogic
{
    /// <summary>
    /// Retriggers the given process step
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    /// <param name="stepToTrigger">The step to retrigger</param>
    /// <param name="mustBePending">If true the offer subscription must be in status pending</param>
    Task TriggerProcessStep(Guid offerSubscriptionId, ProcessStepTypeId stepToTrigger, bool mustBePending = true);

    /// <summary>
    /// Gets the process steps for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    /// <returns>Returns the process steps with their status</returns>
    IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId);
    
    /// <summary>
    /// Gets the service provider company details
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>The detail data</returns>
    Task<ProviderDetailReturnData> GetProviderCompanyDetailsAsync(string iamUserId);

    /// <summary>
    /// Sets service provider company details
    /// </summary>
    /// <param name="data">Detail data for the service provider</param>
    /// <param name="iamUserId">Id of the iam user</param>
    Task SetProviderCompanyDetailsAsync(ProviderDetailData data, string iamUserId);
}
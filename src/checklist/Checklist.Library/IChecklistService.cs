using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public interface IChecklistService
{
    /// <summary>
    /// Creates the wallet for the company of the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="cancellationToken">CanellationToken</param>
    /// <returns>true if the wallet creation was successful, otherwise false.</returns>
    Task<bool> CreateWalletAsync(Guid applicationId, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="iamUserId">the current user</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Updates the company bpn
    /// </summary>
    /// <param name="applicationId">Id of the application to update the bpn</param>
    /// <param name="bpn">the bpn to set</param>
    Task UpdateCompanyBpn(Guid applicationId, string bpn);

    /// <summary>
    /// Processes the possible automated steps of the checklist
    /// </summary>
    /// <param name="applicationId">Id of the application to process the checklist</param>
    /// <param name="checklistEntries">The checklist entries to process</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    Task ProcessChecklist(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> checklistEntries, CancellationToken cancellationToken);
}

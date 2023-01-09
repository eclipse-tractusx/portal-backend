using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;

public interface IChecklistService
{
    /// <summary>
    /// Updates the status of the checklist entry for the given id
    /// </summary>
    /// <param name="applicationId">ID of the checklist entry</param>
    /// <param name="statusId">Id of the new status</param>
    Task UpdateBpnStatusAsync(Guid applicationId, ChecklistEntryStatusId statusId);

    Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken);
}

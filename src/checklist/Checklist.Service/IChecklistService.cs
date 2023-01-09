using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;

public interface IChecklistService
{
    /// <summary>
    /// Creates the initial checklist for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    Task CreateInitialChecklistAsync(Guid applicationId);

    /// <summary>
    /// Updates the status of the checklist entry for the given id
    /// </summary>
    /// <param name="applicationId">ID of the checklist entry</param>
    /// <param name="statusId">Id of the new status</param>
    Task UpdateBpnStatusAsync(Guid applicationId, ChecklistEntryStatusId statusId);

    Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken);
}

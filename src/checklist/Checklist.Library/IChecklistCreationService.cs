using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public interface IChecklistCreationService
{
    /// <summary>
    /// Creates the initial checklist for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    Task CreateInitialChecklistAsync(Guid applicationId);

    /// <summary>
    /// Creates the missing items for the application
    /// </summary>
    /// <remarks>
    /// <b>The DbContext will be cleared</b>
    /// </remarks>
    /// <param name="applicationId">ID of the application the items should be created for</param>
    /// <param name="existingChecklistEntryTypeIds">The currently existing <see cref="ApplicationChecklistEntryTypeId"/></param>
    /// <returns>The created ChecklistApplication Items</returns>
    Task<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>> CreateMissingChecklistItems(Guid applicationId, IEnumerable<ApplicationChecklistEntryTypeId> existingChecklistEntryTypeIds);
}

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public interface IChecklistCreationService
{
    /// <summary>
    /// Creates the initial checklist for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    Task CreateInitialChecklistAsync(Guid applicationId);
}

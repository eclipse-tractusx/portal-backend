using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="AppAssignedLicense"/> entities.
/// </summary>
public interface IAppAssignedLicensesRepository
{
    /// <summary>
    /// Adds an <see cref="AppAssignedLicense"/> to the database
    /// </summary>
    /// <param name="appAssignedLicense">The AppAssignedLicense that should be added to the database</param>
    void AddAppAssignedLicense(AppAssignedLicense appAssignedLicense);
}

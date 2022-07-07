using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Provides functionality to create, modify and get notifications from the persistence layer
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Adds the given notification to the persistence layer.
    /// </summary>
    /// <param name="notification">The notification that should be added to the persistence layer.</param>
    void Add(Notification notification);
}

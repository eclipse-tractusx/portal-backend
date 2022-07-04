using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

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

    /// <summary>
    /// Gets all Notifications for a specific user
    /// </summary>
    /// <param name="companyUserId">Id of the user</param>
    /// <param name="statusId">OPTIONAL: The status of the notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <returns>Returns a collection of NotificationDetailData</returns>
    Task<ICollection<NotificationDetailData>> GetAllAsDetailsByUserIdUntrackedAsync(Guid companyUserId, NotificationStatusId? statusId = null, NotificationTypeId? typeId = null);

    /// <summary>
    /// Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="companyUserId">Id of the receiver</param>
    /// <returns>Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null</returns>
    Task<NotificationDetailData?> GetByIdAndUserIdUntrackedAsync(Guid notificationId, Guid companyUserId);
}

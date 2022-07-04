using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Notification.Service.BusinessLogic;

/// <summary>
/// Business logic to work with the notifications
/// </summary>
public interface INotificationBusinessLogic
{
    /// <summary>
    /// Creates a new Notification with the given data
    /// </summary>
    /// <param name="creationData">The data for the creation of the notification.</param>
    /// <param name="companyUserId">Id of the company user the notification is intended for.</param>
    Task<NotificationDetailData> CreateNotification(NotificationCreationData creationData, Guid companyUserId);

    /// <summary>
    /// Gets all unread notification for the given user.
    /// </summary>
    /// <param name="iamUserId">The id of the current user</param>
    /// <returns>Returns a collection of the users notification</returns>
    Task<ICollection<NotificationDetailData>> GetNotifications(string iamUserId);

    /// <summary>
    /// Gets a specific notification from the database
    /// </summary>
    /// <param name="notificationId">the notification that should be returned</param>
    /// <param name="iamUserId">the id of the current user</param>
    /// <returns>Returns detail data for the given notification</returns>
    Task<NotificationDetailData> GetNotification(Guid notificationId, string iamUserId);
}

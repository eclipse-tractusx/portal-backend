using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

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
}

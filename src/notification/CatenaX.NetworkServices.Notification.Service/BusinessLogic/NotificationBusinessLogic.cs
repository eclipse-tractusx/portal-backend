using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Notification.Service.BusinessLogic;

/// <inheritdoc />
public class NotificationBusinessLogic : INotificationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="NotificationBusinessLogic"/>
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    public NotificationBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData> CreateNotification(NotificationCreationData creationData, Guid companyUserId)
    {
        if (!await _portalRepositories.GetInstance<IUserRepository>().IsUserWithIdExisting(companyUserId))
        {
            throw new ArgumentException("User does not exist", nameof(companyUserId));
        }

        var notificationId = Guid.NewGuid();
        var (dateTimeOffset, title, message, notificationTypeId, notificationStatusId, appId, dueData, creatorId) = creationData;
        CheckEnumValues(notificationTypeId, notificationStatusId);
        this._portalRepositories.GetInstance<INotificationRepository>().Add(new PortalBackend.PortalEntities.Entities.Notification(notificationId, companyUserId, dateTimeOffset, title, message, notificationTypeId, notificationStatusId)
        {
            DueDate = dueData,
            AppId = appId,
            CreatorId = creatorId
        });
        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return new NotificationDetailData(notificationId, title, message);
    }

    /// <summary>
    /// Check the enum values if the api receives a int value which is not in the range of the valid values
    /// </summary>
    /// <param name="notificationTypeId">The notification type</param>
    /// <param name="notificationStatusId">The notification status</param>
    private static void CheckEnumValues(NotificationTypeId notificationTypeId, NotificationStatusId notificationStatusId)
    {
        if(!Enum.IsDefined(typeof(NotificationTypeId), notificationTypeId))
        {
            throw new ArgumentException("notificationType does not exist.", nameof(notificationTypeId));
        }

        if(!Enum.IsDefined(typeof(NotificationStatusId), notificationStatusId.ToString()))
        {
            throw new ArgumentException("notificationStatus does not exist.", nameof(notificationStatusId));
        }
    }
}

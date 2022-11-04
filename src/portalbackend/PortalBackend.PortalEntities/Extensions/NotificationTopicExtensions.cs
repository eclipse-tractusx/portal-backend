using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class NotificationTopicExtensions
{
    public static NotificationTopicId GetNotificationTopic(this NotificationTypeId typeId) =>
        typeId switch
        {
            NotificationTypeId.INFO => NotificationTopicId.INFO,
            NotificationTypeId.TECHNICAL_USER_CREATION => NotificationTopicId.INFO,
            NotificationTypeId.CONNECTOR_REGISTERED => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_SERVICE_PROVIDER => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_USE_CASES => NotificationTopicId.INFO,
            NotificationTypeId.WELCOME_APP_MARKETPLACE => NotificationTopicId.INFO,
            NotificationTypeId.ACTION => NotificationTopicId.ACTION,
            NotificationTypeId.APP_SUBSCRIPTION_REQUEST => NotificationTopicId.ACTION,
            NotificationTypeId.SERVICE_REQUEST => NotificationTopicId.ACTION,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION => NotificationTopicId.OFFER,
            NotificationTypeId.APP_RELEASE_REQUEST => NotificationTopicId.OFFER,
            NotificationTypeId.SERVICE_ACTIVATION => NotificationTopicId.OFFER,
            _ => throw new ArgumentOutOfRangeException(nameof(typeId), typeId, "No NotificationTopicId defined for the given type")
        };
}
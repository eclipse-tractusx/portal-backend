using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Model for the CreateNotification endpoint
/// </summary>
/// <param name="DateCreated">The date the notification was created</param>
/// <param name="Title">The notifications title</param>
/// <param name="Message">The notifications message</param>
/// <param name="NotificationTypeId">The notifications type</param>
/// <param name="ReadStatusId">The notifications status</param>
/// <param name="AppId">OPTIONAL: The linked app for the notification</param>
/// <param name="DueDate">OPTIONAL: The notifications due date</param>
public record NotificationCreationData(DateTimeOffset DateCreated, string Title, string Message, NotificationTypeId  NotificationTypeId, NotificationStatusId ReadStatusId, Guid? AppId, DateTimeOffset? DueDate);

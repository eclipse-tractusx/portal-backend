namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail data of a notification
/// </summary>
/// <param name="Id">The Id of the notification</param>
/// <param name="Title">The notifications title</param>
/// <param name="Message">The notifications message</param>
public record NotificationDetailData(Guid Id, string Title, string Message);

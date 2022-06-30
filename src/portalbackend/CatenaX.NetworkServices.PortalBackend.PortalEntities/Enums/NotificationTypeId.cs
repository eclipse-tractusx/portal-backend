namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

/// <summary>
/// Possible types of a notification
/// </summary>
public enum NotificationTypeId : int
{
    /// <summary>
    /// Notification is just an information for the user
    /// </summary>
    INFO = 1,
    
    /// <summary>
    /// Notification requires the user to take some kind of action
    /// </summary>
    ACTION = 2
}

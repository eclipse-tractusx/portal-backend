using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Type of a notification
/// </summary>
public class NotificationType
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private NotificationType()
    {
        Label = null!;
        Notifications = new HashSet<Notification>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="NotificationType"/> and initializes the id and label 
    /// </summary>
    /// <param name="notificationTypeId">The NotificationTypesId</param>
    public NotificationType(NotificationTypeId notificationTypeId) : this()
    {
        Id = notificationTypeId;
        Label = notificationTypeId.ToString();
    }

    /// <summary>
    /// Id of the type
    /// </summary>
    public NotificationTypeId Id { get; private set; }

    /// <summary>
    /// The type as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Notification> Notifications { get; private set; }
}

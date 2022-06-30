using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Status of a notification
/// </summary>
public class NotificationStatus
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private NotificationStatus()
    {
        Label = null!;
        Notifications = new HashSet<Notification>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="NotificationStatus"/> and initializes the id and label 
    /// </summary>
    /// <param name="notificationStatusId">The NotificationStatusId</param>
    public NotificationStatus(NotificatioStatusId notificationStatusId) : this()
    {
        Id = notificationStatusId;
        Label = notificationStatusId.ToString();
    }

    /// <summary>
    /// Id of the status
    /// </summary>
    public NotificatioStatusId Id { get; private set; }

    /// <summary>
    /// The status as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Notification> Notifications { get; private set; }
}

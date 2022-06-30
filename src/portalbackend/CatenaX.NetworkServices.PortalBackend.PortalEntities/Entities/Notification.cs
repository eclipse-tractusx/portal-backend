using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Notification
/// </summary>
public class Notification
{

    /// <summary>
    /// Internal constuctor, only needed for EF
    /// </summary>
    private Notification() {}

    /// <summary>
    /// Creates a new instance of <see cref="Notification"/> and sets the required values.
    /// </summary>
    /// <param name="id">Id of the notification</param>
    /// <param name="companyUserId">Mapping to the company user</param>
    /// <param name="dateCreated">The creation date</param>
    /// <param name="title">The title of the notification</param>
    /// <param name="message">A custom notification message</param>
    /// <param name="notificationTypeId">id of the notification type</param>
    /// <param name="readStatusId">id of the notification status</param>
    public Notification(Guid id, Guid companyUserId, DateTimeOffset dateCreated, string title, string message, NotificationTypeId notificationTypeId, NotificatioStatusId readStatusId)
    {
        Id = id;
        CompanyUserId = companyUserId;
        DateCreated = dateCreated;
        Title = title;
        Message = message;
        NotificationTypeId = notificationTypeId;
        ReadStatusId = readStatusId;
    }

    public Guid Id { get; private set; }

    public Guid CompanyUserId { get; set; }
    
    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(255)]
    public string Title { get; set; }

    public string Message { get; set; }

    public NotificationTypeId NotificationTypeId { get; set; }
    
    public NotificatioStatusId ReadStatusId { get; set; }

    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual NotificationType? NotificationType { get; set; }
    public virtual NotificationStatus? ReadStatus { get; set; }
}
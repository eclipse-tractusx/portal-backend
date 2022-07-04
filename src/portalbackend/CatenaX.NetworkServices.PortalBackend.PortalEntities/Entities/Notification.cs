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
    private Notification()
    {
        Title = null!;
        Message = null!;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Notification"/> and sets the required values.
    /// </summary>
    /// <param name="id">Id of the notification</param>
    /// <param name="receiverUserId">Mapping to the company user who should receive the message</param>
    /// <param name="dateCreated">The creation date</param>
    /// <param name="title">The title of the notification</param>
    /// <param name="message">A custom notification message</param>
    /// <param name="notificationTypeId">id of the notification type</param>
    /// <param name="readStatusId">id of the notification status</param>
    public Notification(Guid id, Guid receiverUserId, DateTimeOffset dateCreated, string title, string message, NotificationTypeId notificationTypeId, NotificationStatusId readStatusId)
    {
        Id = id;
        ReceiverUserId = receiverUserId;
        DateCreated = dateCreated;
        Title = title;
        Message = message;
        NotificationTypeId = notificationTypeId;
        ReadStatusId = readStatusId;
    }

    public Guid Id { get; private set; }

    public Guid ReceiverUserId { get; private set; }
    
    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(150)]
    public string Title { get; private set; }

    public string Message { get; private set; }

    public NotificationTypeId NotificationTypeId { get; private set; }
    
    public NotificationStatusId ReadStatusId { get; private set; }

    public Guid? AppId { get; set; }
    
    public DateTimeOffset? DueDate { get; set; }

    public Guid? CreatorUserId { get; set; }

    // Navigation properties
    public virtual App? App { get; private set; }
    public virtual CompanyUser? Receiver { get; private set; }
    public virtual NotificationType? NotificationType { get; private set; }
    public virtual NotificationStatus? ReadStatus { get; private set; }
    public virtual CompanyUser? Creator { get; private set; }
}

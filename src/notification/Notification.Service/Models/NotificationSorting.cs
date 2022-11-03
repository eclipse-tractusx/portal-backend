namespace Notification.Service.Models;

/// <summary>
/// Possible sorting options for the notification pagination
/// </summary>
public enum NotificationSorting
{
    /// <summary>
    /// Ascending by date
    /// </summary>
    DateAsc = 1,
    
    /// <summary>
    /// Descending by date
    /// </summary>
    DateDesc = 2,
    
    /// <summary>
    /// Ascending by the read status
    /// </summary>
    ReadStatusAsc = 3,
    
    /// <summary>
    /// Descending by read status
    /// </summary>
    ReadStatusDesc = 4,
}
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class NotificationRepository : INotificationRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="NotificationRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public NotificationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public void Add(Notification notification)
    {
        _dbContext.Add(notification);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<NotificationDetailData> GetAllAsDetailsByUserIdUntracked(Guid companyUserId, NotificationStatusId? statusId = null, NotificationTypeId? typeId = null) =>
        _dbContext.Notifications
            .Where(x =>
                x.ReceiverUserId == companyUserId &&
                statusId.HasValue ? x.ReadStatusId == statusId.Value : true &&
                typeId.HasValue ? x.NotificationTypeId == typeId.Value : true)
            .Select(x => new NotificationDetailData(x.Id, x.Title, x.Message))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public async Task<NotificationDetailData?> GetByIdAndUserIdUntrackedAsync(Guid notificationId, Guid companyUserId) =>
        await _dbContext.Notifications
            .Where(x => x.Id == notificationId && x.ReceiverUserId == companyUserId)
            .Select(x => new NotificationDetailData(x.Id, x.Title, x.Message))
            .SingleOrDefaultAsync();
}

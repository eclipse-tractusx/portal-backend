using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

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
}

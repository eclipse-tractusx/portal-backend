using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Extensions;

public static class NotificationQueryExtension
{
    public static IOrderedQueryable<Notification> GetNotificationOrderByQuery(this IQueryable<Notification> source, NotificationSorting sorting) =>
        sorting switch
        {
            NotificationSorting.DateAsc => source.OrderBy(notification => notification.DateCreated),
            NotificationSorting.ReadStatusAsc => source.OrderBy(notification => notification.IsRead),
            NotificationSorting.ReadStatusDesc => source.OrderByDescending(notification => notification.IsRead),
            NotificationSorting.DateDesc => source.OrderByDescending(notification => notification.DateCreated),
            _ => throw new InvalidCastException("The sorting does not exists.")
        };
}
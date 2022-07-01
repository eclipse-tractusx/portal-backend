using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// View model containing the ID of a company and its app subscription status in a specific context.
/// </summary>
public record CompanySubscriptionStatusData(Guid CompanyId, AppSubscriptionStatusId AppSubscriptionStatus);
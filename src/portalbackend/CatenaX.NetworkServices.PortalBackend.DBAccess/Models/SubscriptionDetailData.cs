using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail Data for the subscription
/// </summary>
public record SubscriptionDetailData(Guid OfferId, string OfferName, OfferSubscriptionStatusId Status);
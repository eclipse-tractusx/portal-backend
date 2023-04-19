using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail data for the offer subscription
/// </summary>
/// <param name="Status">Offer status</param>
/// <param name="CompanyUserId">Company User Id</param>
/// <param name="TechnicalUserId">Id of the company</param>
/// <param name="CompanyName">Name of the provider company</param>
/// <param name="CompanyId">Id of the company</param>
/// <param name="RequesterId">Id of the requester for the offer subscription</param>
/// <param name="OfferId">Id of the offer</param>
/// <param name="OfferName">Name of the offer</param>
/// <param name="Bpn">Bpn of the app company</param>
/// <param name="RequesterEmail">Email address of the requesting company user</param>
/// <param name="RequesterFirstname">First name of the requesting company user</param>
/// <param name="RequesterLastname">First name of the requesting company user</param>
/// <param name="InstanceData">Defines whether the offer is a single or multiple instance offer</param>
/// <param name="AppInstanceIds">Ids of the app instances</param>
public record OfferSubscriptionTransferData(OfferSubscriptionStatusId Status,
    Guid CompanyUserId,
    Guid TechnicalUserId,
    string CompanyName,
    Guid CompanyId,
    Guid RequesterId,
    Guid OfferId,
    string? OfferName,
    string? Bpn,
    string? RequesterEmail,
    string? RequesterFirstname,
    string? RequesterLastname, 
    (bool IsSingleInstance, string? InstanceUrl) InstanceData,
    IEnumerable<Guid> AppInstanceIds
);

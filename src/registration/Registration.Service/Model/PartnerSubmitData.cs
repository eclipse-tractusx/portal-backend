using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

public record PartnerSubmitData
(
    IEnumerable<CompanyRoleId> CompanyRoles,
    IEnumerable<AgreementConsentData> Agreements
);

/// <summary>
/// 
/// </summary>
/// <param name="AgreementId"></param>
/// <param name="ConsentStatusId"></param>
/// <returns></returns>
public record AgreementConsentData(
    Guid AgreementId,
    [property: JsonPropertyName("consentStatus")] ConsentStatusId ConsentStatusId
);

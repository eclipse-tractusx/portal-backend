using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record PartnerSubmitData
(
    IEnumerable<CompanyRoleId> CompanyRoles,
    IEnumerable<AgreementConsentData> Agreements
);

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record TestDataRegistrationModel(
    CompanyDetailData? CompanyDetailData,
    CompanyDetailData? UpdateCompanyDetailData,
    List<CompanyRoleId> CompanyRoles,
    string? DocumentTypeId,
    string? DocumentName
);

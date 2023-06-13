using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public record TestDataModel(
    CompanyDetailData companyDetailData, CompanyDetailData? updateCompanyDetailData, List<CompanyRoleId>? companyRoles, string? documentTypeId, string? documentPath);
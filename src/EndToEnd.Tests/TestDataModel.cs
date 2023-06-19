using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

namespace EndToEnd.Tests;

public record TestDataModel(
    CompanyDetailData companyDetailData, CompanyDetailData? updateCompanyDetailData, List<CompanyRoleId>? companyRoles, string? documentTypeId, string? documentPath);
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using RestAssured.Request.Logging;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public class AdditionalEndpoints
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;
    
    public AdditionalEndpoints()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
    }

    [Fact]
    public void GetCompanyDetailData_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Log(RequestLogLevel.All)
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyDetailData));
        Assert.NotEqual(0, data.CompanyId.ToString().Length);
    }

    [Fact]
    public void GetApplicationStatus_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyApplicationStatusId)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyApplicationStatusId));
        Assert.True(Enum.IsDefined(typeof(CompanyApplicationStatusId), data));
    }

    [Fact]
    public void GetAgreementConsentStatuses_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyRoleAgreementConsents)Given()
            .Log(RequestLogLevel.All)
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyRoleAgreementConsents));
    }

    [Fact]
    public void GetInvitedUsers_ReturnsExpectedResult()
    {
        // Given
        var data = (InvitedUser[])Given()
            .RelaxedHttpsValidation()
            .Log(RequestLogLevel.All)
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(InvitedUser[]));
        Assert.NotEmpty(data);
    }
}
using Castle.Core.Internal;
using EndToEnd.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "PortalHC")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
[Collection("PortalHC")]
public class RegistrationHealthCheck : EndToEndTestBase
{
    private readonly string _baseUrl = TestResources.BasePortalBackendUrl;
    private readonly string _endPoint = "/api/registration";
    private readonly string _portalUserCompanyName = TestResources.PortalUserCompanyName;
    private static string? PortalUserToken;

    private static readonly Secrets Secrets = new();

    public RegistrationHealthCheck(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetAccessToken() // in order to just get token once, ensure that method name is alphabetically before other tests cases
    {
        PortalUserToken = await new AuthFlow(_portalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);

        PortalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");
    }

    [Fact]
    public void GetCompanyRoles()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/company/companyRoles")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public void GetCompanyRoleAgreementData()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/companyRoleAgreementData")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = response.Content.ReadAsStringAsync().Result;
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public void GetClientRolesComposite()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/rolesComposite")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    [Fact]
    public void GetApplicationsWithStatus()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract().Response();
        var data = response.Content.ReadAsStringAsync().Result;
        data.Should().NotBeNullOrEmpty("Response body is null or empty");
    }
}

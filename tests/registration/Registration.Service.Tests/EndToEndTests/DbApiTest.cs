using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.EndToEndTests.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class DbApiTest
{
    private readonly string _baseUrl = TestResources.BaseUrl;
    private readonly string _endPoint = "/api/registration";
    private readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;
    private static string _portalUserToken;

    private static readonly Secrets Secrets = new();

    #region DB / API Test

    [Fact]
    public async Task Test0_Setup_GetToken()
    {
        _portalUserToken = await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);
    }

    [Fact]
    public void Test1_GetCompanyRoles_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/company/companyRoles")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }


    [Fact]
    public void Test2_GetCompanyRoleAgreementData_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/companyRoleAgreementData")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        var data = response.Content.ReadAsStringAsync().Result;
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    [Fact]
    public void Test3_GetClientRolesComposite_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/rolesComposite")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    [Fact]
    public void Test4_GetApplicationsWithStatus_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract().Response();
        var data = response.Content.ReadAsStringAsync().Result;
        Assert.NotNull(data);
        Assert.NotEmpty(data);
    }

    #endregion
}
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using Xunit;
using static RestAssured.Dsl;

namespace EndToEnd.Tests;

[TestCaseOrderer("EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
public class SystemHealthCheckEndPointTests
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();
    private static string? _portalUserToken;
    private static readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    [Fact]
    public async Task Test0_Setup_GetToken()
    {
        _portalUserToken = await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);
    }

    // GET: /api/administration/staticdata/usecases
    [Fact]
    public void Test1_GetUseCaseData_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/usecases")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        
        Assert.NotEmpty(response.Content.ReadAsStringAsync().Result);
        Assert.NotNull(response.Content.ReadAsStringAsync().Result);
    }

    //     GET: /api/administration/staticdata/languagetags
    [Fact]
    public void Test2_GetAppLanguageTags_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/languagetags")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        
        Assert.NotEmpty(response.Content.ReadAsStringAsync().Result);
        Assert.NotNull(response.Content.ReadAsStringAsync().Result);
    }

    //     GET: /api/administration/staticdata/licenseType
    [Fact]
    public void Test3_GetAllLicenseTypes_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/licenseType")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        
        Assert.NotEmpty(response.Content.ReadAsStringAsync().Result);
        Assert.NotNull(response.Content.ReadAsStringAsync().Result);
    }

    //     GET: api/administration/user/owncompany/users
    [Fact]
    public void Test4_GetCompanyUserData_ReturnsExpectedResult()
    {
        var response = Given()
            . DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/user/owncompany/users?page=0&size=5")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        
        Assert.NotEmpty(response.Content.ReadAsStringAsync().Result);
        Assert.NotNull(response.Content.ReadAsStringAsync().Result);
    }
    
    //     GET: api/administration/companydata/ownCompanyDetails
    [Fact]
    public void Test5_GetOwnCompanyDetails_ReturnsExpectedResult()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/companydata/ownCompanyDetails")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        
        Assert.NotEmpty(response.Content.ReadAsStringAsync().Result);
        Assert.NotNull(response.Content.ReadAsStringAsync().Result);
    }
}
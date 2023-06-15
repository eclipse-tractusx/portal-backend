using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public class SystemHealthCheckEndPointTests
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();
    private static string? _operatorToken;
    private static readonly string OperatorCompanyName = TestResources.OperatorCompanyName;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task GetOperatorToken()
    {
        _operatorToken =
            await new AuthFlow(OperatorCompanyName).GetAccessToken(Secrets.OperatorUserName,
                Secrets.OperatorUserPassword);
    }

    // GET: /api/administration/staticdata/usecases
    [Fact]
    public async Task GetUseCaseData_ReturnsExpectedResult()
    {
        await GetOperatorToken();
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
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
    public async Task GetAppLanguageTags_ReturnsExpectedResult()
    {
        await GetOperatorToken();
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
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
    public async Task GetAllLicenseTypes_ReturnsExpectedResult()
    {
        await GetOperatorToken();
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
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
    public async Task GetCompanyUserData_ReturnsExpectedResult()
    {
        await GetOperatorToken();
        var response = Given()
            . DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
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
    public async Task GetOwnCompanyDetails_ReturnsExpectedResult()
    {
        await GetOperatorToken();
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
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
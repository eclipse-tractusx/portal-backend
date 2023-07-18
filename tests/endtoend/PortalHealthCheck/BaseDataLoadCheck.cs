using Castle.Core.Internal;
using EndToEnd.Tests;
using FluentAssertions;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "PortalHC")]
[TestCaseOrderer("Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests.AlphabeticalOrderer",
    "EndToEnd.Tests")]
[Collection("PortalHC")]
public class BaseDataLoadCheck : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();
    private static string? PortalUserToken;
    private static readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;

    public BaseDataLoadCheck(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetAccessToken() // in order to just get token once, ensure that method name is alphabetically before other tests cases
    {
        PortalUserToken = await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);

        PortalUserToken.Should().NotBeNullOrEmpty("Token for the portal user could not be fetched correctly");
    }

    // GET: /api/administration/staticdata/usecases
    [Fact]
    public void GetUseCaseData()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/usecases")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Error: Response body is null or empty");
    }

    //     GET: /api/administration/staticdata/languagetags
    [Fact]
    public void GetAppLanguageTags()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/languagetags")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: /api/administration/staticdata/licenseType
    [Fact]
    public void GetAllLicenseTypes()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/staticdata/licenseType")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: api/administration/user/owncompany/users
    [Fact]
    public void GetCompanyUserData()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/user/owncompany/users?page=0&size=5")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }

    //     GET: api/administration/companydata/ownCompanyDetails
    [Fact]
    public void GetOwnCompanyDetails()
    {
        if (PortalUserToken.IsNullOrEmpty())
            throw new Exception("Portal user token could not be fetched");

        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/companydata/ownCompanyDetails")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        response.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response body is null or empty");
    }
}

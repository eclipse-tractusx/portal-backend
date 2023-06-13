using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmEndToEndTests
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/catena/legal-entities";
    private static string? _interfaceHealthCheckTechUserToken;

    private static readonly Secrets Secrets = new ();

    [Fact]
    public void BpdmInterfaceHealthCheck_ReturnsExpectedResult()
    {
        RetrieveHealthCheckTechUserToken();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.totalElements");
        Assert.NotEqual(0, response);
    }

    private void RetrieveHealthCheckTechUserToken()
    {
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", Secrets.InterfaceHealthCheckTechUserPassword),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", Secrets.InterfaceHealthCheckTechUserName),
        };

        _interfaceHealthCheckTechUserToken = Given()
            .ContentType("application/x-www-form-urlencoded")
            .FormData(formData)
            .When()
            .Post("https://centralidp.dev.demo.catena-x.net/auth/realms/CX-Central/protocol/openid-connect/token")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.access_token").ToString();
    }
}
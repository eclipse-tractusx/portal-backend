using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using static RestAssured.Dsl;
namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmEndpointTests
{
    private static readonly string _baseUrl = "https://partners-pool.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/catena/legal-entities";

    private static readonly Secrets _secrets = new ();

    [Fact]
    public void BpdmInterfaceHealthCheck_ReturnsExpectedResult()
    {
        string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.totalElements");
            //.Response();
        //Assert.Contains(response.Content.ReadAsStringAsync());
        Assert.NotEqual(0, response);
    }
    
    private string? RetrieveHealthCheckTechUserToken()
    {
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", _secrets.InterfaceHealthCheckTechUserPassword),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", _secrets.InterfaceHealthCheckTechUserName),
        };
      
      
        var interfaceHealthCheckTechUserToken = Given()
            .ContentType("application/x-www-form-urlencoded")
            .FormData(formData)
            .When()
            .Post("https://centralidp.dev.demo.catena-x.net/auth/realms/CX-Central/protocol/openid-connect/token")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.access_token").ToString();
        return interfaceHealthCheckTechUserToken;
    }
}
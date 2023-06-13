using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Tests;
using static RestAssured.Dsl;

public class ClearinghouseEndToEndTests
{
    private static readonly string BaseUrl = TestResources.ClearingHouseUrl;
    private static readonly string EndPoint = "/api/v1/validation ";

    private static readonly Secrets Secrets = new ();

    [Fact]
    public void ClearinghouseInterfaceHealthCheck_ReturnsExpectedResult()
    {
        string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Body(
                "{\"callbackUrl\":\"https://portal-backend.dev.demo.catena-x.net/api/administration/registration/clearinghouse\",\"participantDetails\":{\"name\":\"SmokeTest CH\",\"city\":\"Stuttgart\",\"street\":\"Test Street\",\"bpn\":\"BPNL000SMOKE0011\",\"region\":\"Bavaria\",\"zipCode\":\"01108\",\"country\":\"Germany\",\"countryAlpha2Code\":\"DE\"},\"identityDetails\":{\"did\":\"did:sov:RPgthNMDkVdzYQhXzahh3P\",\"uniqueIds\":[{\"type\":\"local\",\"value\":\"HB8272819\",}]}}")
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .StatusCode(200);
            // .And()
            // .Extract()
            // .Body("$.totalElements");
        //.Response();
        //Assert.Contains(response.Content.ReadAsStringAsync());
        //Assert.NotEqual(0, response);
    }
    
    private string? RetrieveHealthCheckTechUserToken()
    {
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", Secrets.InterfaceHealthCheckTechUserPassword),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", Secrets.InterfaceHealthCheckTechUserName),
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



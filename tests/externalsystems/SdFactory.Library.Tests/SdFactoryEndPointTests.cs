using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryEndpointTests
{
    private static readonly string BaseUrl = "https://sdfactory.dev.demo.catena-x.net";
    private static readonly string EndPoint = "/api/rel3/selfdescription";
    private static readonly string WalletBaseUrl = "https://managed-identity-wallets.dev.demo.catena-x.net";
    private static readonly string WalletEndPoint = "/api/wallets";
    private static string? _interfaceHealthCheckTechUserToken;

    private static readonly Secrets Secrets = new ();

    [Fact]
    public void InterfaceHealthCheck_ReturnsExpectedResult()
    {
        RetrieveHealthCheckTechUserToken();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Body(
                "{\"externalId\": \"TestAutomation\",\"type\": \"LegalPerson\",\"holder\": \"BPNL000000000000\",\"issuer\": \"CAXSDUMMYCATENAZZ\",\"registrationNumber\": [{\"type\": \"local\",\"value\": \"o12345678\"}], \"headquarterAddress.country\": \"DE\",\"legalAddress.country\": \"DE\",\"bpn\": \"BPNL000000000000\"}")
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .StatusCode(202);
    }

    //GET https://managed-identity-wallets.dev.demo.catena-x.net/api/wallets
    [Fact]
    public void GetListOfWallets_ReturnsExpectedResult()
    {
        RetrieveHealthCheckTechUserToken();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Get($"{WalletBaseUrl}{WalletEndPoint}")
            .Then()
            .StatusCode(200);
    }

    [Fact]
    //POST /api/wallets
    public void CreateWalletFirstTime_ReturnsExpectedResult()
    {
        RetrieveHealthCheckTechUserToken();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Body("{\"bpn\": \"BPNL000000000001\", \"name\": \"bpn\"}")
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .StatusCode(201);
    }

    [Fact]
    //POST /api/wallets
    public void CreateWalletSecondTime_ReturnsExpectedResult()
    {
        RetrieveHealthCheckTechUserToken();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Body("{\"bpn\": \"BPNL000000000000\", \"name\": \"bpn\"}")
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .StatusCode(409);
    }

    [Fact]
    public void RetrieveHealthCheckTechUserToken()
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
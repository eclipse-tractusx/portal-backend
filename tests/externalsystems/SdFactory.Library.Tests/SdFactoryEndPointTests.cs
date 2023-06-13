using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryEndpointTests
{
    private static readonly string BaseUrl = TestResources.SdFactoryBaseUrl;
    private static readonly string EndPoint = "/api/rel3/selfdescription";
    private static readonly string WalletBaseUrl = TestResources.WalletBaseUrl;
    private static readonly string WalletEndPoint = "/api/wallets";
    private static string? _interfaceHealthCheckTechUserToken;
    private static string? _bpn;

    private static readonly Secrets Secrets = new ();

    [Fact]
    public void InterfaceHealthCheckSdDocCreation_ReturnsExpectedResult()
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

    [Fact]
    public void Scenario_InterfaceHealthCheckWalletCreation()
    {
        _bpn = $"TestAutomation_{DateTime.Now:s}";
        RetrieveHealthCheckTechUserToken();
        GetListOfWallets_ReturnsExpectedResult();
        Thread.Sleep(3000);
        CreateWallet_ReturnsExpectedResult(201);
        Thread.Sleep(3000);
        CreateWallet_ReturnsExpectedResult(409);
    }

    //GET https://managed-identity-wallets.dev.demo.catena-x.net/api/wallets
    private void GetListOfWallets_ReturnsExpectedResult()
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

    //POST /api/wallets
    private void CreateWallet_ReturnsExpectedResult(int statusCode)
    {
        RetrieveHealthCheckTechUserToken();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Body($"{{\"bpn\": \"{_bpn}\", \"name\": \"bpn\"}}")
            .Post($"{WalletBaseUrl}{WalletEndPoint}")
            .Then()
            .StatusCode(statusCode);
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
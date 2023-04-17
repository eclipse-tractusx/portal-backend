using Castle.Core.Internal;
using EndToEnd.Tests;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class WalletEndpointTests : EndToEndTestBase
{
    private static readonly string WalletBaseUrl = TestResources.WalletBaseUrl;

    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    private const string WalletEndPoint = "/api/wallets";
    private static string? InterfaceHealthCheckTechUserToken;
    private static string? Bpn;

    private static readonly Secrets Secrets = new();

    public WalletEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Scenario_InterfaceHealthCheckWalletCreation()
    {
        Bpn = $"TestAutomation_{DateTime.Now:s}";
        InterfaceHealthCheckTechUserToken =
            TechTokenRetriever.GetToken(TokenUrl, Secrets.InterfaceHealthCheckTechClientId,
                Secrets.InterfaceHealthCheckTechClientSecret);
        if (InterfaceHealthCheckTechUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for interface partner health check");
        GetListOfWallets_ReturnsExpectedResult();
        Thread.Sleep(3000);
        CreateWallet_ReturnsExpectedResult(201);
        Thread.Sleep(3000);
        CreateWallet_ReturnsExpectedResult(409);
    }

    //GET: /api/wallets
    private static void GetListOfWallets_ReturnsExpectedResult()
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {InterfaceHealthCheckTechUserToken}")
            .When()
            .Get($"{WalletBaseUrl}{WalletEndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200);
    }

    //POST: /api/wallets
    private static void CreateWallet_ReturnsExpectedResult(int statusCode)
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {InterfaceHealthCheckTechUserToken}")
            .When()
            .Body($"{{\"bpn\": \"{Bpn}\", \"name\": \"bpn\"}}")
            .Post($"{WalletBaseUrl}{WalletEndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(statusCode);
    }
}

using Castle.Core.Internal;
using EndToEnd.Tests;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class SdFactoryEndpointTests : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.SdFactoryBaseUrl;

    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    private const string EndPoint = "/api/rel3/selfdescription";
    private string? InterfaceHealthCheckTechUserToken;

    private static readonly Secrets Secrets = new();

    public SdFactoryEndpointTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void InterfaceHealthCheckSdDocCreation_ReturnsExpectedResult()
    {
        InterfaceHealthCheckTechUserToken = TechTokenRetriever.GetToken(TokenUrl,
            Secrets.InterfaceHealthCheckTechClientId,
            Secrets.InterfaceHealthCheckTechClientSecret);
        if (InterfaceHealthCheckTechUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for interface partner health check");

        var body = DataHandleHelper.SerializeData(
            new SdFactoryRequestModel(
                "TestAutomation",
                new List<RegistrationNumber> { new("local", "o12345678") },
                "DE",
                "DE",
                SdFactoryRequestModelSdType.LegalParticipant,
                "BPNL000000000000",
                "BPNL000000000000",
                "CAXSDUMMYCATENAZZ"
            )
        );

        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {InterfaceHealthCheckTechUserToken}")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(202);
    }
}

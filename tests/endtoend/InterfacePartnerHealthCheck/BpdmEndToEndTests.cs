using Castle.Core.Internal;
using EndToEnd.Tests;
using FluentAssertions;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class BpdmEndToEndTests : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.BpdmUrl;

    private static readonly string TokenUrl =
        TestResources.BaseCentralIdpUrl + "/auth/realms/CX-Central/protocol/openid-connect/token";

    private const string EndPoint = "/api/catena/legal-entities";
    private string? _interfaceHealthCheckTechUserToken;

    private static readonly Secrets Secrets = new();

    public BpdmEndToEndTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void BpdmInterface_HealthCheck()
    {
        _interfaceHealthCheckTechUserToken =
            TechTokenRetriever.GetToken(TokenUrl,
                Secrets.InterfaceHealthCheckTechClientId,
                Secrets.InterfaceHealthCheckTechClientSecret);
        if (_interfaceHealthCheckTechUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for interface partner health check");
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_interfaceHealthCheckTechUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.totalElements");
        response.Should().NotBe(0);
    }
}

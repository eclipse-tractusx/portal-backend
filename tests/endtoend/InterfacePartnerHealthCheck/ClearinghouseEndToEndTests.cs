using Castle.Core.Internal;
using EndToEnd.Tests;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using RestAssured.Response.Logging;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "InterfaceHC")]
[Collection("InterfaceHC")]
public class ClearinghouseEndToEndTests : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.ClearingHouseUrl;
    private static readonly string BaseTokenUrl = TestResources.ClearingHouseTokenUrl;
    private const string EndPoint = "/api/v1/validation";
    private string? ClearingHouseUserToken;

    private static readonly Secrets Secrets = new();

    public ClearinghouseEndToEndTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void ClearinghouseInterface_HealthCheck()
    {
        ClearingHouseUserToken =
            TechTokenRetriever.GetToken(BaseTokenUrl,
                Secrets.ClearingHouseClientId,
                Secrets.ClearingHouseClientSecret);
        if (ClearingHouseUserToken.IsNullOrEmpty())
            throw new Exception("Could not fetch token for clearing house health check.");

        var body = DataHandleHelper.SerializeData(
            new ClearinghouseTransferData(
                new ParticipantDetails(
                    "SmokeTest CH", "Stuttgart", "Test Street", "BPNL000SMOKE0011", "Bavaria", "01108",
                    "Germany", "DE"
                ),
                new IdentityDetails(
                    "did:sov:RPgthNMDkVdzYQhXzahh3P", // hardcode due to initial requirements in CPLP-2803
                    new List<UniqueIdData> { new("local", "HB8272819") }
                ),
                $"{TestResources.BasePortalBackendUrl}/api/administration/registration/clearinghouse",
                false)
        );

        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {ClearingHouseUserToken}")
            .When()
            .Body(body)
            .Post($"{BaseUrl}{EndPoint}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        data.Content.ReadAsStringAsync().Result.Should().NotBeNullOrEmpty("Response should not be null or empty");
    }
}

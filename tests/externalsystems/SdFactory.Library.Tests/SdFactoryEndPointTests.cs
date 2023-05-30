using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using static RestAssured.Dsl;
namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests.RestAssured;

public class SdFactoryEndpointTests
{
  private static readonly string _baseUrl = "https://sdfactory.dev.demo.catena-x.net";
  private static readonly string _endPoint = "/api/rel3/selfdescription";
  
  private static readonly string _walletBaseUrl = "https://managed-identity-wallets.dev.demo.catena-x.net";
  private static readonly string _walletEndPoint = "/api/wallets";
  
  private static readonly Secrets _secrets = new ();
  
  [Fact]
  public void InterfaceHealthCheck_ReturnsExpectedResult()
  {
    string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
    Given()
      .RelaxedHttpsValidation()
      .Header(
        "authorization",
        $"Bearer {_interfaceHealthCheckTechUserToken}")
      .When()
      .Body("{\"externalId\": \"TestAutomation\",\"type\": \"LegalPerson\",\"holder\": \"BPNL000000000000\",\"issuer\": \"CAXSDUMMYCATENAZZ\",\"registrationNumber\": [{\"type\": \"local\",\"value\": \"o12345678\"}], \"headquarterAddress.country\": \"DE\",\"legalAddress.country\": \"DE\",\"bpn\": \"BPNL000000000000\"}")
      .Post($"{_baseUrl}{_endPoint}")
      .Then()
      .StatusCode(202);

  }
    
    //GET https://managed-identity-wallets.dev.demo.catena-x.net/api/wallets
    [Fact]
    public void GetListOfWallets_ReturnsExpectedResult()
    {
      string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
      Given()
        .RelaxedHttpsValidation()
        .Header(
          "authorization",
          $"Bearer {_interfaceHealthCheckTechUserToken}")
        .When()
        .Get($"{_walletBaseUrl}{_walletEndPoint}")
        .Then()
        .StatusCode(200);
    }
    
    [Fact]
    //POST /api/wallets
    public void CreateWalletFirstTime_ReturnsExpectedResult()
    {
      string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
      Given()
        .RelaxedHttpsValidation()
        .Header(
          "authorization",
          $"Bearer {_interfaceHealthCheckTechUserToken}")
        .When()
        .Body("{\"bpn\": \"BPNL000000000001\", \"name\": \"bpn\"}")
        .Post($"{_baseUrl}{_endPoint}")
        .Then()
        .StatusCode(201);
    }

    [Fact]
    //POST /api/wallets
    public void CreateWalletSecondTime_ReturnsExpectedResult()
    {
      string? _interfaceHealthCheckTechUserToken = RetrieveHealthCheckTechUserToken();
      Given()
        .RelaxedHttpsValidation()
        .Header(
          "authorization",
          $"Bearer {_interfaceHealthCheckTechUserToken}")
        .When()
        .Body("{\"bpn\": \"BPNL000000000000\", \"name\": \"bpn\"}")
        .Post($"{_baseUrl}{_endPoint}")
        .Then()
        .StatusCode(409);
    }

    [Fact]
    public string? RetrieveHealthCheckTechUserToken()
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
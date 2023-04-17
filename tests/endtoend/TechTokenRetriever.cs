using RestAssured.Response.Logging;
using System.IdentityModel.Tokens.Jwt;
using static RestAssured.Dsl;

namespace EndToEnd.Tests;

public static class TechTokenRetriever
{
    public static string GetToken(string tokenUrl, string clientId, string? clientSecret)
    {
        if (clientSecret is null)
        {
            throw new Exception("No client secret provided while trying to get a token.");
        }
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", clientId),
        };

        var token = Given()
            .ContentType("application/x-www-form-urlencoded")
            .FormData(formData)
            .When()
            .Post(tokenUrl)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.access_token").ToString();
        return token ?? throw new Exception("No token received");
    }
}

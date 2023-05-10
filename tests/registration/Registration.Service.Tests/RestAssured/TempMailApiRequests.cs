using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;
namespace Registration.Service.Tests.RestAssured;

public class TempMailApiRequests
{
    private readonly string _baseUrl = "https://api.apilayer.com";
    private readonly string _endPoint = "/temp_mail";
    private readonly string _tempMailApiKey;
    
    public TempMailApiRequests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _tempMailApiKey = configuration.GetValue<string>("Secrets:TempMailApiKey");
    }

    [Fact]
    public string GetDomain()
    {
        var data = (string[])Given()
            .RelaxedHttpsValidation()
            .Header(
                "apikey",
                $"{_tempMailApiKey}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/domains")
            .Then() 
            .StatusCode(200)
            .Extract()
            .As(typeof(string[]));
        return data[0];
    }
}
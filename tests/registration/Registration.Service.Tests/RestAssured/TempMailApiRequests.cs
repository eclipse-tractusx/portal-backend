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
    private readonly string _apiTestUsername = "apitestuser";
    
    public TempMailApiRequests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _tempMailApiKey = configuration.GetValue<string>("Secrets:TempMailApiKey");
    }

    private string GetDomain()
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


    [Fact]
    public void CheckMailbox()
    {
        var hashedEmailAddress = CreateMd5();
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "apikey",
                $"{_tempMailApiKey}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/mail/id/{hashedEmailAddress}")
            .Then()
            .StatusCode(200)
            .Extract().As(typeof(List<TempMailMessageData>));
        //.As(typeof(string[]));
        //return data[0];
    }

    //https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
    private string CreateMd5()
    {
        var emailAddress = _apiTestUsername + GetDomain();
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(emailAddress);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }

    //TODO: Delete all messages in mailbox after getting password
    /*
    private void DeleteMessagesInMailbox()
    {
        foreach (var mailId in mailIds)
        {
            var data = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "apikey",
                    $"{_tempMailApiKey}")
                .When()
                .Get($"{_baseUrl}{_endPoint}/delete/id/{mailId}")
                .Then()
                .StatusCode(200)
                .Extract().Body();
        }
    }*/
}
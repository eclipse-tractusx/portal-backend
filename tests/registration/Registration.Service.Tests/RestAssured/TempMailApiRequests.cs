using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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
    private readonly Secrets _secrets = new ();

    [Fact]
    public string? FetchPassword()
    {
        TempMailMessageData? passwordMessage = GetPasswordMessage();

        if (passwordMessage != null)
        {
            //search for password in the first span of the body
            Regex r = new Regex(@"<span[^>].*?>([^<]*)<\/span>", RegexOptions.IgnoreCase);
            string password = HttpUtility.HtmlDecode(r.Matches(passwordMessage.mail_html).First().Groups[1].Value.Trim('\n').Trim());
            DeletePasswordMessage(passwordMessage.mail_id);
            return password;
        }
        
        return null;
    }

    private TempMailMessageData? GetPasswordMessage()
    {
        var hashedEmailAddress = CreateMd5();
        var messages = (List<TempMailMessageData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/mail/id/{hashedEmailAddress}")
            .Then()
            .StatusCode(200)
            .Extract().As(typeof(List<TempMailMessageData>));
        var passwordMessage = messages.First(item => item.mail_subject.Contains("Password required"));
        return passwordMessage;
    }
    
    public string GetDomain()
    {
        var data = (string[])Given()
            .RelaxedHttpsValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/domains")
            .Then() 
            .StatusCode(200)
            .Extract()
            .As(typeof(string[]));
        return data[1];
    }
    

    //https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
    private string CreateMd5()
    {
        var emailAddress = _apiTestUsername + GetDomain();
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(emailAddress);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }

    private void DeletePasswordMessage(string mailId)
    {
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "apikey",
                $"{_secrets.TempMailApiKey}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/delete/id/{mailId}")
            .Then()
            .StatusCode(200);
    }
}
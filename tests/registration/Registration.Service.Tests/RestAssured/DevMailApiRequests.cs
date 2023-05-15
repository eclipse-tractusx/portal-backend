using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using RestAssured.Request.Logging;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured;

public class DevMailApiRequests
{
    private readonly string _mailServiceBaseUrl = "https://developermail.com/api/v1"; 
    private static DevMailboxData _devMailboxData;
    
    public DevMailApiRequests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
    }
    
    [Fact]
    public string? FetchPassword()
    {
        DevMailboxContent emails = CheckMailbox();
        var index = emails.Result.FindIndex(item => item.Value.Contains("Password required"));
        string? password = null;

        if (index >= 0)
        {
            var message = new MimeMessage();
            byte[] byteArray = Encoding.ASCII.GetBytes(emails.Result[index].Value);
            MemoryStream stream = new MemoryStream(byteArray);
            message = MimeMessage.Load(stream);
        
            //search for password in the first span of the body
            Regex r = new Regex(@"<span[^>].*?>([^<]*)<\/span>", RegexOptions.IgnoreCase);
        
            password = HttpUtility.HtmlDecode(r.Match(message.HtmlBody).Groups[1].Value.Trim('\n').Trim());
        }

        DeleteMailbox();

        return password;
    }
    
    [Fact]
    public DevMailboxData GenerateRandomEmailAddress()
    {
        _devMailboxData = (DevMailboxData) Given()
            .RelaxedHttpsValidation()
            .When()
            .Put($"{_mailServiceBaseUrl}/mailbox")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(DevMailboxData));
        return _devMailboxData;
    }

    [Fact]
    private DevMailboxContent CheckMailbox()
    {
        var messageIds = GetMessageIds();
        var emails = (DevMailboxContent)    Given()
            .Log(RequestLogLevel.All)
            .RelaxedHttpsValidation()
            .Header(
                "X-MailboxToken",
                 $"{_devMailboxData.Result.Token}")
            .Body(messageIds)
            .When()
            .Post(
                $"{_mailServiceBaseUrl}/mailbox/{_devMailboxData.Result.Name}/messages")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(DevMailboxContent));

        return emails;
    }

    [Fact]
    private void DeleteMailbox()
    {
        var result = (bool)Given()
            .RelaxedHttpsValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData.Result.Token}")
            .When()
            .Delete($"{_mailServiceBaseUrl}/mailbox/{_devMailboxData.Result.Name}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Body("$.result");
        Assert.True(result);
    }

    [Fact]
    private string[] GetMessageIds()
    {
        var messageIdsResult = (DevMailboxMessageIds)Given()
            .Log(RequestLogLevel.All)
            .RelaxedHttpsValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData.Result.Token}")
            .When()
            .Get(
                $"{_mailServiceBaseUrl}/mailbox/{_devMailboxData.Result.Name}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .As(typeof(DevMailboxMessageIds));
        return messageIdsResult.Result;
    }
}
using EndToEnd.Tests;
using HtmlAgilityPack;
using MimeKit;
using System.Text;
using System.Web;
using Xunit;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class DevMailApiRequests
{
    private const string MailServiceBaseUrl = "https://developermail.com/api/v1";

    private DevMailboxData? _devMailboxData;

    public string? FetchPassword()
    {
        var emails = CheckMailbox();
        var index = emails.Result.FindIndex(item => item.Value.Contains("Password required"));
        string? password = null;

        if (index < 0)
            throw new Exception("Authentication flow failed: email with password could not be found.");
        var byteArray = Encoding.ASCII.GetBytes(emails.Result[index].Value);
        var stream = new MemoryStream(byteArray);
        var message = MimeMessage.Load(stream);

        var doc = new HtmlDocument();
        doc.LoadHtml(message.HtmlBody);

        var pNodes = doc.DocumentNode.SelectNodes("//p");

        //search for password in the first paragraph after the paragraph with "Below you can find you password"
        for (var i = 0; i < pNodes.Count - 1; i++)
        {
            if (!pNodes[i].InnerText.Trim().Contains("Below you can find your password"))
                continue;
            password = HttpUtility.HtmlDecode(pNodes[i + 1].InnerText.Trim());
            break;
        }

        DeleteMailbox();

        return password;
    }

    public DevMailboxData GenerateRandomEmailAddress()
    {
        const string Endpoint = "/mailbox";
        var data = Given()
            .DisableSslCertificateValidation()
            .When()
            .Put(MailServiceBaseUrl + Endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();
        _devMailboxData = DataHandleHelper.DeserializeData<DevMailboxData>(data.Content.ReadAsStringAsync().Result);
        if (_devMailboxData is null)
        {
            throw new Exception($"Could not get mail address from {Endpoint}, response was null/empty.");
        }
        return _devMailboxData;
    }

    private DevMailboxContent CheckMailbox()
    {
        var messageIds = GetMessageIds();
        var endpoint = $"/mailbox/{_devMailboxData?.Result.Name}/messages";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .Body(messageIds)
            .When()
            .Post(MailServiceBaseUrl + endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();

        var emails = DataHandleHelper.DeserializeData<DevMailboxContent>(data.Content.ReadAsStringAsync().Result);
        if (emails is null)
        {
            throw new Exception($"Could not mails from {endpoint}, response data was null.");
        }
        return emails;
    }

    private void DeleteMailbox()
    {
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .When()
            .Delete($"{MailServiceBaseUrl}/mailbox/{_devMailboxData?.Result.Name}")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Body("$.result");
    }

    private string[] GetMessageIds()
    {
        var endpoint = $"/mailbox/{_devMailboxData?.Result.Name}";
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "X-MailboxToken",
                $"{_devMailboxData?.Result.Token}")
            .When()
            .Get(MailServiceBaseUrl + endpoint)
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();

        var messageIds = DataHandleHelper.DeserializeData<DevMailboxMessageIds>(data.Content.ReadAsStringAsync().Result);
        if (messageIds is null)
        {
            throw new Exception($"Could not messageIds from {endpoint}, response data was null.");
        }
        return messageIds.Result;
    }
}

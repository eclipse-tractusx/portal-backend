namespace Registration.Service.Tests.RestAssured;

public class OneSecMailApiRequests
{
    // private string GenerateRandomEmailAddress()
    // {
    //     // Given
    //     var emailAddress = (string[])Given()
    //         .RelaxedHttpsValidation()
    //         .When()
    //         .Get("https://www.1secmail.com/api/v1/?action=genRandomMailbox&count=1")
    //         .Then()
    //         .And()
    //         .StatusCode(200)
    //         .Extract().As(typeof(string[]));
    //     _userEmailAddress = emailAddress[0].Split("@");
    //     return emailAddress[0];
    // }
    //
    // private MailboxData[] CheckMailBox()
    // {
    //     // Given
    //     var emails = (MailboxData[])Given()
    //         .RelaxedHttpsValidation()
    //         .When()
    //         .Get(
    //             $"https://www.1secmail.com/api/v1/?action=getMessages&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}")
    //         .Then()
    //         .And()
    //         .StatusCode(200)
    //         .Extract().As(typeof(MailboxData[]));
    //     return emails;
    // }
    //
    // private EmailMessageData? FetchPassword()
    // {
    //     // Given
    //     var emails = CheckMailBox();
    //     if (emails.Length != 0)
    //     {
    //         var passwordMail = emails[0]?.Id;
    //         var messageData = (EmailMessageData)Given()
    //             .RelaxedHttpsValidation()
    //             .When()
    //             .Get(
    //                 $"https://www.1secmail.com/api/v1/?action=readMessage&login={_userEmailAddress[0]}&domain={_userEmailAddress[1]}&id={passwordMail}")
    //             .Then()
    //             .And()
    //             .StatusCode(200)
    //             .Extract().As(typeof(EmailMessageData));
    //         return messageData;
    //     }
    //
    //     return null;
    // }
}
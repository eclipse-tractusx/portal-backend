namespace Registration.Service.Tests.RestAssured;

public record MailboxData(
    int Id,
    string From,
    string Subject,
    string Date
);
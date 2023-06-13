namespace Tests.Shared.RestAssured.AuthFlow;

public record MailboxData(
    int Id,
    string From,
    string Subject,
    string Date
);
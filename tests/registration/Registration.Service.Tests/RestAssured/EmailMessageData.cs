namespace Registration.Service.Tests.RestAssured;

public record EmailMessageData(
    int Id,
    string From,
    string Subject,
    string Date,
    IEnumerable<EmailAttachment> Attachments,
    string Body,
    string TextBody,
    string HtmlBody
);

public record EmailAttachment(
    string Filename,
    string ContentType,
    int Size
);
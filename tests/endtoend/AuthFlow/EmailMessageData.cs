using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

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

public record TempMailMessageData(
    [property: JsonPropertyName("_id")] TempMailMessageId Id,
    [property: JsonPropertyName("createdAt")] TempMailMessageCreatedAt CreatedAt,
    [property: JsonPropertyName("mail_address_id")] string MailAddressId,
    [property: JsonPropertyName("mail_attachments")] string[] MailAttachments,
    [property: JsonPropertyName("mail_attachments_count")] int MailAttachmentsCount,
    [property: JsonPropertyName("mail_from")] string MailFrom,
    [property: JsonPropertyName("mail_html")] string MailHtml,
    [property: JsonPropertyName("mail_id")] string MailId,
    [property: JsonPropertyName("mail_preview")] string MailPreview,
    [property: JsonPropertyName("mail_subject")] string MailSubject,
    [property: JsonPropertyName("mail_text")] string MailText,
    [property: JsonPropertyName("mail_text_only")] string MailTextOnly,
    [property: JsonPropertyName("mail_timstamp")] string MailTimestamp
);

public record TempMailMessageId(
    [property: JsonPropertyName("$oid")] string Oid);

public record TempMailMessageCreatedAt(
    [property: JsonPropertyName("$date")] TempMailMessageNumberLong Date
    );

public record TempMailMessageNumberLong(
    [property: JsonPropertyName("$numberLong")] string NumberLong);

using Newtonsoft.Json;

namespace Tests.Shared.RestAssured.AuthFlow;

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
    TempMailMessageId _id,
    TempMailMessageCreatedAt createdAt,
    string mail_address_id,
    string[] mail_attachments,
    int mail_attachments_count,
    string mail_from,
    string mail_html,
    string mail_id,
    string mail_preview,
    string mail_subject,
    string mail_text,
    string mail_text_only,
    string mail_timstamp
);

public record TempMailMessageId(
    [JsonProperty(PropertyName = "$oid")] string oid);
    
public record TempMailMessageCreatedAt(
    [JsonProperty(PropertyName = "$date")] TempMailMessageNumberLong date
    );
        
public record TempMailMessageNumberLong(
     [JsonProperty(PropertyName = "$numberLong")] string numberLong);
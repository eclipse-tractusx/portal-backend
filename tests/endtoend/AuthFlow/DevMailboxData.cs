using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public record DevMailboxData(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("errors")] string Errors,
    [property: JsonPropertyName("result")] MailboxResultData Result
);

public record MailboxResultData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("token")] string Token
);

public record DevMailboxContent(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("errors")] string Errors,
    [property: JsonPropertyName("result")] List<MailboxContent> Result
);

public record MailboxContent(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string Value
);

public record DevMailboxMessageIds(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("errors")] string Errors,
    [property: JsonPropertyName("result")] string[] Result
);


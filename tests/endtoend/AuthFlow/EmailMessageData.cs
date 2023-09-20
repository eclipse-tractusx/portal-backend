/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

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

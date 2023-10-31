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


/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record DocumentSeedData(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("date_created"), JsonConverter(typeof(JsonDateTimeOffsetConverter))] DateTimeOffset DateCreated,
    [property: JsonPropertyName("document_name")] string DocumentName,
    [property: JsonPropertyName("document_type_id")] int DocumentTypeId,
    [property: JsonPropertyName("company_user_id")] Guid? CompanyUserId,
    [property: JsonPropertyName("document_hash")] byte[] DocumentHash,
    [property: JsonPropertyName("document_content")] byte[] DocumentContent,
    [property: JsonPropertyName("document_status_id")] int DocumentStatusId
);

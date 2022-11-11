/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Agreement Data
/// </summary>
/// <param name="AgreementId">Id of the agreement</param>
/// <param name="AgreementName">Name of the agreement</param>
public record AgreementData(
    [property: JsonPropertyName("agreementId")] Guid AgreementId,
    [property: JsonPropertyName("name")] string AgreementName);

/// <summary>
/// Agreement Assigned Document Data
/// </summary>
/// <param name="AgreementId">Id of the agreement</param>
/// <param name="AgreementName">Name of the agreement</param>
/// <param name="DocumentIds">Ids of the documents</param>
public record AgreementDocumentData(
    [property: JsonPropertyName("agreementId")] Guid AgreementId,
    [property: JsonPropertyName("name")] string AgreementName,
    [property: JsonPropertyName("documentIds")] IEnumerable<Guid> DocumentIds);
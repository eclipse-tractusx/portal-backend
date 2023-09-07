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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;

/// <summary>
/// Callback data for the offer provider after the auto setup succeeded
/// </summary>
/// <param name="ExternalId">External id of the registration</param>
/// <param name="ApplicationId">Id of the company application</param>
/// <param name="Bpn">Companies bpn</param>
/// <param name="Status">Status of the application</param>
/// <param name="Message">OPTIONAL: Additional Message</param>
public record OnboardingServiceProviderCallbackData(
    [property: JsonPropertyName("externalId")] Guid ExternalId,
    [property: JsonPropertyName("applicationId")] Guid ApplicationId,
    [property: JsonPropertyName("bpn")] string Bpn,
    [property: JsonPropertyName("status")] CompanyApplicationStatusId? Status,
    [property: JsonPropertyName("message")] string? Message
);

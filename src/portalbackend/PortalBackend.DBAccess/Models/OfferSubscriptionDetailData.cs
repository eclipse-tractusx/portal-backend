/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record SubscriptionTechnicalUserData(
    Guid Id,
    string? Name,
    IEnumerable<string> Permissions
);

/// <summary>
/// Detail data for a offer subscription
/// </summary>
/// <param name="Id">Id of the Offer</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
/// <param name="Name">Name of the Offer</param>
/// <param name="Customer">Name of the company subscribing the offer</param>
/// <param name="Bpn">When called from /provider bpn of the company subscribing the offer, otherwise the provider company's bpn</param>
/// <param name="Contact">When called from /provider the company admins of the subscribing company, otherwise the salesmanagers of the offer provider</param>
/// <param name="TechnicalUserData">Information about the technical user</param>
public record ProviderSubscriptionDetailData(
    Guid Id,
    OfferSubscriptionStatusId OfferSubscriptionStatus,
    string? Name,
    string Customer,
    string? Bpn,
    IEnumerable<string> Contact,
    IEnumerable<SubscriptionTechnicalUserData> TechnicalUserData
);

/// <summary>
/// Detail data for a offer subscription
/// </summary>
/// <param name="Id">Id of the Offer</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
/// <param name="Name">Name of the Offer</param>
/// <param name="Customer">Name of the company subscribing the offer</param>
/// <param name="Bpn">When called from /provider bpn of the company subscribing the offer, otherwise the provider company's bpn</param>
/// <param name="Contact">When called from /provider the company admins of the subscribing company, otherwise the salesmanagers of the offer provider</param>
/// <param name="TechnicalUserData">Information about the technical user</param>
/// <param name="TenantUrl">Url of Tenant</param>
/// <param name="AppInstanceId">Id of the app instance</param>
public record AppProviderSubscriptionDetailData(
    Guid Id,
    OfferSubscriptionStatusId OfferSubscriptionStatus,
    string? Name,
    string Customer,
    string? Bpn,
    IEnumerable<string> Contact,
    IEnumerable<SubscriptionTechnicalUserData> TechnicalUserData,
    IEnumerable<SubscriptionAssignedConnectorData> ConnectorData,
    string? TenantUrl,
    string AppInstanceId,
    ProcessStepTypeId? ProcessStepTypeId,
    SubscriptionExternalServiceData ExternalService
);

public record SubscriptionExternalServiceData(
    [property: JsonPropertyName("trusted_issuer")] string TrustedIssuer,
    [property: JsonPropertyName("participant_id")] string? ParticipantId,
    [property: JsonPropertyName("iatp_id")] string? IatpId,
    [property: JsonPropertyName("did_resolver")] string DidResolver,
    [property: JsonPropertyName("decentralIdentityManagementAuthUrl")] string DecentralIdentityManagementAuthUrl,
    [property: JsonPropertyName("decentralIdentityManagementServiceUrl")] string? DecentralIdentityManagementServiceUrl
);

/// <summary>
/// Detail data for a offer subscription
/// </summary>
/// <param name="Id">Id of the Offer</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
/// <param name="Name">Name of the Offer</param>
/// <param name="Provider">The provider company's name</param>
/// <param name="Contact">When called from /provider the company admins of the subscribing company, otherwise the salesmanagers of the offer provider</param>
/// <param name="TechnicalUserData">Information about the technical user</param>
public record SubscriberSubscriptionDetailData(
    Guid Id,
    OfferSubscriptionStatusId OfferSubscriptionStatus,
    string? Name,
    string Provider,
    IEnumerable<string> Contact,
    IEnumerable<SubscriptionTechnicalUserData> TechnicalUserData,
    IEnumerable<SubscriptionAssignedConnectorData> ConnectorData
);

/// <summary>
/// offer subscription assigned connector data details
/// </summary>
/// <param name="ConnectorId">Id of the connector</param>
/// <param name="ConnectorName">Name of the connector</param>
/// <param name="ConnectorUrl">Url of the connector</param>
public record SubscriptionAssignedConnectorData(
    [property: JsonPropertyName("id")] Guid ConnectorId,
    [property: JsonPropertyName("name")] string ConnectorName,
    [property: JsonPropertyName("endpoint")] string ConnectorUrl);

/// <summary>
/// Detail data for a offer subscription
/// </summary>
/// <param name="Id">Id of the Offer</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
/// <param name="Name">Name of the Offer</param>
/// <param name="Customer">Name of the company subscribing the offer</param>
/// <param name="Bpn">When called from /provider bpn of the company subscribing the offer, otherwise the provider company's bpn</param>
/// <param name="Contact">When called from /provider the company admins of the subscribing company, otherwise the salesmanagers of the offer provider</param>
/// <param name="TechnicalUserData">Information about the technical user</param>
/// <param name="TenantUrl">Url of Tenant</param>
/// <param name="AppInstanceId">Id of the app instance</param>
public record AppProviderSubscriptionDetail(
    Guid Id,
    OfferSubscriptionStatusId OfferSubscriptionStatus,
    string? Name,
    string Customer,
    string? Bpn,
    IEnumerable<string> Contact,
    IEnumerable<SubscriptionTechnicalUserData> TechnicalUserData,
    string? TenantUrl,
    string AppInstanceId,
    IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId)> ProcessSteps,
    IEnumerable<SubscriptionAssignedConnectorData> ConnectorData,
    ExternalServiceData? ExternalServiceData
);

public record ExternalServiceData(
    [property: JsonPropertyName("trusted_issuer")] string TrustedIssuer,
    [property: JsonPropertyName("participant_id")] string? ParticipantId,
    [property: JsonPropertyName("decentralIdentityManagementServiceUrl")] string DecentralIdentityManagementServiceUrl
);

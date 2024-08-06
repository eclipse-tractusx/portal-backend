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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model for connectors.
/// </summary>
public record ConnectorData(
    string Name,
    [StringLength(2, MinimumLength = 2)]
    string Location,
    Guid Id,
    ConnectorTypeId Type,
    ConnectorStatusId Status,
    Guid? HostId,
    string? HostCompanyName,
    Guid? SelfDescriptionDocumentId,
    TechnicalUserData? TechnicalUser,
    string ConnectorUrl
);

/// <summary>
/// Connector information for the daps call.
/// </summary>
public record ConnectorInformationData(
    string Name,
    string Bpn,
    Guid Id,
    string Url);

/// <summary>
/// View model for connectors.
/// </summary>
public record ManagedConnectorData(
    string Name,
    [StringLength(2, MinimumLength = 2)]
    string Location,
    Guid Id,
    ConnectorTypeId Type,
    ConnectorStatusId Status,
    string? ProviderCompanyName,
    Guid? SelfDescriptionDocumentId,
    TechnicalUserData? TechnicalUser,
    string ConnectorUrl);

/// <summary>
/// connector information to delete
/// </summary>
public record DeleteConnectorData(
    bool IsProvidingOrHostCompany,
    Guid? SelfDescriptionDocumentId,
    DocumentStatusId? DocumentStatusId,
    ConnectorStatusId ConnectorStatus,
    IEnumerable<ConnectorOfferSubscription> ConnectorOfferSubscriptions,
    UserStatusId? UserStatusId,
    Guid? ServiceAccountId
);
public record ConnectorOfferSubscription(Guid AssignedOfferSubscriptionIds, OfferSubscriptionStatusId OfferSubscriptionStatus);

public record TechnicalUserData(Guid Id, string Name, string? ClientId, string Description);

public record ConnectorMissingSdDocumentData(Guid Id, string Name, Guid CompanyId, string CompanyName);

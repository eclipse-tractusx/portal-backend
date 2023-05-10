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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail data for the offer subscription
/// </summary>
/// <param name="Status">Offer status</param>
/// <param name="CompanyUserId">Company User Id</param>
/// <param name="TechnicalUserId">Id of the company</param>
/// <param name="CompanyName">Name of the provider company</param>
/// <param name="CompanyId">Id of the company</param>
/// <param name="RequesterId">Id of the requester for the offer subscription</param>
/// <param name="OfferId">Id of the offer</param>
/// <param name="OfferName">Name of the offer</param>
/// <param name="Bpn">Bpn of the app company</param>
/// <param name="RequesterEmail">Email address of the requesting company user</param>
/// <param name="RequesterFirstname">First name of the requesting company user</param>
/// <param name="RequesterLastname">First name of the requesting company user</param>
/// <param name="InstanceData">Defines whether the offer is a single or multiple instance offer</param>
/// <param name="AppInstanceIds">Ids of the app instances</param>
/// <param name="SalesManagerId">Id of the sales manager</param>
public record OfferSubscriptionTransferData(OfferSubscriptionStatusId Status,
    Guid CompanyUserId,
    Guid TechnicalUserId,
    string CompanyName,
    Guid CompanyId,
    Guid RequesterId,
    Guid OfferId,
    string? OfferName,
    string? Bpn,
    string? RequesterEmail,
    string? RequesterFirstname,
    string? RequesterLastname, 
    (bool IsSingleInstance, string? InstanceUrl) InstanceData,
    IEnumerable<Guid> AppInstanceIds,
    Guid? SalesManagerId
);

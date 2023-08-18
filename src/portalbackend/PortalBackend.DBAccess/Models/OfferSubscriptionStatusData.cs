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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Offer Subscriptions Status Data
/// </summary>
/// <param name="OfferId">Id of the Offer</param>
/// <param name="OfferName">Name of the Offer</param>
/// <param name="Provider">When called from /provider name of the company subscribing the offer, otherwise the provider company's name</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
/// <param name="DocumentId">Id of the documents</param>
public record OfferSubscriptionStatusData(
    [property: JsonPropertyName("offerId")] Guid OfferId,
    [property: JsonPropertyName("name")] string? OfferName,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("status")] OfferSubscriptionStatusId OfferSubscriptionStatusId,
    [property: JsonPropertyName("image")] Guid? DocumentId
);

/// <summary>
/// Offer Subscription data
/// </summary>
/// <param name="OfferId">Id of the Offer</param>
/// <param name="OfferSubscriptionStatus">Status of the offer subscription</param>
public record OfferSubscriptionData(
    [property: JsonPropertyName("offerId")] Guid OfferId,
    [property: JsonPropertyName("status")] OfferSubscriptionStatusId OfferSubscriptionStatusId
);

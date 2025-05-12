/********************************************************************************
 * Copyright (c) 2023 BMW Group AG
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
/// Get offer subscription with offer provider
/// </summary>
/// <param name="Exists">Existence of offer subscription</param>
/// <param name="IsOfferProvider">true if Company Id is the same as ProviderCompanyId</param>
/// <param name="OfferSubscriptionAlreadyLinked">true if OfferSubscription is already linked to a connector</param>
/// <param name="OfferSubscriptionStatus">Offer subscription status</param>
/// <param name="SelfDescriptionDocumentId">Provider company sd document id</param>
/// <param name="CompanyId">Host/Customer company id</param>
/// <param name="ProviderBpn">Provider's Bpn</param>
/// <param name="CountryAlpha2Code">Provider's country code</param>
public record OfferSubscriptionWithProvider(
    bool Exists,
    bool IsOfferProvider,
    bool OfferSubscriptionAlreadyLinked,
    OfferSubscriptionStatusId OfferSubscriptionStatus,
    Guid? SelfDescriptionDocumentId,
    Guid CompanyId,
    string? ProviderBpn,
    string? CountryAlpha2Code
);

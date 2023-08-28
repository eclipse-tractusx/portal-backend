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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Data Object for the AppProvider Details
/// </summary>
/// <param name="OfferName">Name of the offer</param>
/// <param name="ProviderName">Name of the provider</param>
/// <param name="ProviderContactEmail">Contact email of the provider</param>
/// <param name="SalesManagerId">Id of the sales manager for the app</param>
/// <param name="AutoSetupUrl">Url for the autosetup</param>
/// <param name="IsSingleInstance">Defines if the offer is a single instance offer</param>
public record OfferProviderDetailsData(
    string? OfferName,
    string ProviderName,
    string? ProviderContactEmail,
    Guid? SalesManagerId,
    string? AutoSetupUrl,
    bool IsSingleInstance,
    Guid? ProviderCompanyId
);

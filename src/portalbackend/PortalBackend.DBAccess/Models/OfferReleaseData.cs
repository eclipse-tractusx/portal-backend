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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Data Object for the Offer Release data
/// </summary>
/// <param name="Name"></param>
/// <param name="ThumbnailUrl"></param>
/// <param name="ProviderCompanyId"></param>
/// <param name="CompanyName"></param>
/// <param name="IsDescriptionLongNotSet"></param>
/// <param name="IsDescriptionShortNotSet"></param>
/// <returns></returns>
public record OfferReleaseData(
    string? Name,
    string? ThumbnailUrl,
    Guid? ProviderCompanyId,
    string CompanyName,
    bool IsDescriptionLongNotSet,
    bool IsDescriptionShortNotSet);

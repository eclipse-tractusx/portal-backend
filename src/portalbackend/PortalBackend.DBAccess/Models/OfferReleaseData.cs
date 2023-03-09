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
/// Data Object for the Offer Release data
/// </summary>
/// <param name="Name"></param>
/// <param name="ProviderCompanyId"></param>
/// <param name="CompanyName"></param>
/// <param name="IsDescriptionLongNotSet"></param>
/// <param name="IsDescriptionShortNotSet"></param>
/// <param name="HasUserRoles"></param>
/// <param name="DocumentStatusDatas"></param>
/// <param name="DocumentTypeIds"></param>
/// <returns></returns>
public record OfferReleaseData(
    string? Name,
    Guid? ProviderCompanyId,
    string CompanyName,
    bool IsDescriptionLongNotSet,
    bool IsDescriptionShortNotSet,
    bool HasUserRoles,
    IEnumerable<DocumentStatusData?> DocumentStatusDatas,
    IEnumerable<DocumentTypeId> DocumentTypeIds
);

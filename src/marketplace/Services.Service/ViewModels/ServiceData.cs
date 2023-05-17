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

using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

/// <summary>
/// View model of an application's detailed data specific for service.
/// </summary>
/// <param name="Id">ID of the service.</param>
/// <param name="Title">Title or name of the service.</param>
/// <param name="ServiceTypes">Collection of the assigned serviceTypeIds</param>
/// <param name="Provider">Provider of the service.</param>
/// <param name="Descriptions">The description of the service.</param>
/// <param name="Documents">documents assigned to service.</param>
/// <param name="ProviderUri">Provider Uri of the service</param>
/// <param name="ContactEmail">Contact Email of the service.</param>
/// <param name="ContactNumber">Contact Number of the service. </param>
/// <param name="LicenseType">License Type for offer </param>
/// <param name="OfferStatus">Status of the offer </param>
/// <param name="TechnicalUserProfile">Status of the offer </param>
public record ServiceData(
    Guid Id,
    string Title,
    IEnumerable<ServiceTypeId> ServiceTypes,
    string Provider,
    IEnumerable<LocalizedDescription> Descriptions,
    IDictionary<DocumentTypeId, IEnumerable<DocumentData>> Documents,
    string ProviderUri,
    string? ContactEmail,
    string? ContactNumber,
    LicenseTypeId LicenseType,
    OfferStatusId OfferStatus,
    IDictionary<Guid, IEnumerable<string>> TechnicalUserProfile
);

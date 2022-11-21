/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Data object to create a new service offering
/// </summary>
/// <param name="Title">title of the service offering</param>
/// <param name="Price">the price</param>
/// <param name="ThumbnailUrl">url of the thumbnail</param>
/// <param name="ContactEmail">contact email address</param>
/// <param name="SalesManager">the sales manager of the service</param>
/// <param name="Descriptions">the service descriptions</param>
public record OfferingData(
    string Title,
    string Price,
    string? ThumbnailUrl,
    string? ContactEmail,
    Guid SalesManager,
    IEnumerable<OfferingDescription> Descriptions);

/// <summary>
/// Data object to create a new service offering
/// </summary>
/// <param name="Title">title of the service offering</param>
/// <param name="Price">the price</param>
/// <param name="ThumbnailUrl">url of the thumbnail</param>
/// <param name="ContactEmail">contact email address</param>
/// <param name="SalesManager">the sales manager of the service</param>
/// <param name="Descriptions">the service descriptions</param>
/// <param name="ServiceTypeIds">service type ids</param>
public record ServiceOfferingData(
    string Title,
    string Price,
    string? ThumbnailUrl,
    string? ContactEmail,
    Guid SalesManager,
    IEnumerable<OfferingDescription> Descriptions,
    IEnumerable<ServiceTypeId> ServiceTypeIds);

/// <summary>
/// Description of a service
/// </summary>
/// <param name="LanguageCode">the language code (2-chars)</param>
/// <param name="Description">the service description</param>
public record OfferingDescription(string LanguageCode, string Description);

/// <summary>
/// Data object for the service agreement consent
/// </summary>
/// <param name="AgreementId">Id of the agreement</param>
/// <param name="ConsentStatusId">Id of the consent status</param>
public record OfferAgreementConsentData(Guid AgreementId, ConsentStatusId ConsentStatusId);

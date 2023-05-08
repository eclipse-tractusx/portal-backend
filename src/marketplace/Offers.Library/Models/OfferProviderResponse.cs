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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

/// <summary>
/// Response for the offer creation
/// </summary>
/// <param name="Title">title of the offer</param>
/// <param name="Provider">provider name</param>
/// <param name="LeadPictureId">id of the lead picture</param>
/// <param name="ProviderName">provider name</param>
/// <param name="UseCase">list of use cases</param>
/// <param name="Descriptions">the offer descriptions</param>
/// <param name="Agreements">the assigned agreements</param>
/// <param name="SupportedLanguageCodes">the supported language codes</param>
/// <param name="Price">the offer price</param>
/// <param name="Images">list of the images</param>
/// <param name="ProviderUri">the provider uri</param>
/// <param name="ContactEmail">contact email</param>
/// <param name="ContactNumber">contact number</param>
/// <param name="Documents">list of linked documents</param>
/// <param name="SalesManagerId">id of the salesmanager</param>
/// <param name="PrivacyPolicies">the privacy policies</param>
/// <param name="ServiceTypeIds">ids of the service types</param>
/// <param name="TechnicalUserProfile">ids of the service types</param>
public record OfferProviderResponse(
    string? Title,
    string Provider,
    Guid LeadPictureId,
    string? ProviderName,
    IEnumerable<AppUseCaseData>? UseCase,
    IEnumerable<LocalizedDescription> Descriptions,
    IEnumerable<OfferAgreement> Agreements,
    IEnumerable<string> SupportedLanguageCodes,
    string? Price,
    IEnumerable<Guid> Images,
    string? ProviderUri,
    string? ContactEmail,
    string? ContactNumber,
    IDictionary<DocumentTypeId, IEnumerable<DocumentData>> Documents,
    Guid? SalesManagerId,
    IEnumerable<PrivacyPolicyId> PrivacyPolicies,
    IEnumerable<ServiceTypeId>? ServiceTypeIds,
    IDictionary<Guid, IEnumerable<string>> TechnicalUserProfile
);

/// <summary>
/// Model for Agreement and Consent Status
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="ConsentStatus"></param>
/// <returns></returns>
public record OfferAgreement(Guid? Id, string? Name, string? ConsentStatus);

/// <summary>
/// Model for Document
/// </summary>
/// <param name="documentId"></param>
/// <param name="documentName"></param>
public record DocumentData(Guid documentId, string documentName);

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

public record OfferProviderResponse(
    string? Title, 
    string Provider, 
    Guid LeadPictureId, 
    string? ProviderName, 
    IEnumerable<AppUseCaseData> UseCase, 
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
    IEnumerable<PrivacyPolicyId> PrivacyPolicies
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

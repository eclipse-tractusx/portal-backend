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
/// View model of an application's detailed data.
/// </summary>
/// <param name="Id">ID of the app.</param>
/// <param name="Title">Title or name of the app.</param>
/// <param name="LeadPictureId">Id of the Lead Image.</param>
/// <param name="Images">List of Images to app's secondary pictures</param>
/// <param name="ProviderUri">Uri to provider's marketing presence</param>
/// <param name="Provider">Provider of the app</param>
/// <param name="ContactEmail">Email address of the app's primary contact</param>
/// <param name="ContactNumber">Phone number of the app's primary contact</param>
/// <param name="UseCases">Names of the app's use cases</param>
/// <param name="LongDescription">Long description of the app</param>
/// <param name="Price">Pricing information of the app</param>
/// <param name="Tags">Tags assigned to application</param>
/// <param name="IsSubscribed">Whether app has been purchased by the user's company</param>
/// <param name="Languages">Languages that the app is available in</param>
/// <param name="Documents">document assigned to offer</param>
/// <param name="PrivacyPolicies">privacy policy assigned to offer</param>
public record OfferDetailsData(
    Guid Id,
    string? Title,
    Guid LeadPictureId,
    IEnumerable<Guid> Images,
    string? ProviderUri,
    string Provider,
    string? ContactEmail,
    string? ContactNumber,
    IEnumerable<AppUseCaseData> UseCases,
    string? LongDescription,
    string? Price,
    IEnumerable<string> Tags,
    OfferSubscriptionStatusId IsSubscribed,
    IEnumerable<string> Languages,
    IEnumerable<DocumentTypeData> Documents,
    IEnumerable<PrivacyPolicyId> PrivacyPolicies
);

/// <summary>
/// Model for Document with Type
/// </summary>
/// <param name="documentTypeId"></param>
/// <param name="documentId"></param>
/// <param name="documentName"></param>
public record DocumentTypeData(DocumentTypeId documentTypeId, Guid documentId, string documentName);

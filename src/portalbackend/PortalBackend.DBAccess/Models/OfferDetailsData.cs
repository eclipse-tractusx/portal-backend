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
/// View model of an application's detailed data.
/// </summary>
public record OfferDetailsData(

    /// <summary>
    /// ID of the app.
    /// </summary>
    Guid Id,

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    string? Title,

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    string? LeadPictureUri,

    /// <summary>
    /// List of URIs to app's secondary pictures.
    /// </summary>
    IEnumerable<string> DetailPictureUris,

    /// <summary>
    /// Uri to provider's marketing presence.
    /// </summary>
    string? ProviderUri,

    /// <summary>
    /// Provider of the app.
    /// </summary>
    string Provider,

    /// <summary>
    /// Email address of the app's primary contact.
    /// </summary>
    string? ContactEmail,

    /// <summary>
    /// Phone number of the app's primary contact.
    /// </summary>
    string? ContactNumber,

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    IEnumerable<string> UseCases,

    /// <summary>
    /// Long description of the app.
    /// </summary>
    string? LongDescription,

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    string? Price,

    /// <summary>
    /// Tags assigned to application.
    /// </summary>
    IEnumerable<string> Tags,

    /// <summary>
    /// Whether app has been purchased by the user's company.
    /// </summary>
    OfferSubscriptionStatusId IsSubscribed,

    /// <summary>
    /// Languages that the app is available in.
    /// </summary>
    IEnumerable<string> Languages,
    /// <summary>
    /// document assigned to offer
    /// </summary>
    IEnumerable<DocumentTypeData> Documents
);

/// <summary>
/// Model for Document with Type
/// </summary>
/// <param name="documentTypeId"></param>
/// <param name="documentId"></param>
/// <param name="documentName"></param>
public record DocumentTypeData(DocumentTypeId documentTypeId, Guid documentId, string documentName);

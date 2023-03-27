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

public record InReviewOfferData(

    /// <summary>
    /// ID of the app.
    /// </summary>
    Guid id,

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    string? title,

    /// <summary>
    /// Id of the Lead Image.
    /// </summary>
    Guid leadPictureId,

    /// <summary>
    /// List of Images to app's secondary pictures.
    /// </summary>
    IEnumerable<Guid> images,

    /// <summary>
    /// Provider of the app.
    /// </summary>
    string Provider,

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    IEnumerable<string> UseCases,

    /// <summary>
    /// Descriptions of the app's
    /// </summary>
    IEnumerable<LocalizedDescription> Description,

    /// <summary>
    /// document assigned to offer
    /// </summary>
    IEnumerable<DocumentTypeData> Documents,
    
    /// <summary>
    /// Roles of the Apps
    /// </summary>
    IEnumerable<string> Roles,

    /// <summary>
    /// Languages that the app is available in.
    /// </summary>
    IEnumerable<string> Languages,

    /// <summary>
    /// Uri to provider's marketing presence.
    /// </summary>
    string? ProviderUri,

    /// <summary>
    /// Email address of the app's primary contact.
    /// </summary>
    string? ContactEmail,

    /// <summary>
    /// Phone number of the app's primary contact.
    /// </summary>
    string? ContactNumber,

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    string? Price,

    /// <summary>
    /// Tags assigned to application.
    /// </summary>
    IEnumerable<string> Tags
);

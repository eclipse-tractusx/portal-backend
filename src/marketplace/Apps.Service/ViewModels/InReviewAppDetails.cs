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

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// View model of an application's detailed data.
/// </summary>
/// <param name="Id">ID of the app.</param>
/// <param name="Title">Title or name of the app.</param>
/// <param name="LeadPictureId">Uri to app's lead picture.</param>
/// <param name="Images">List of Images to app's secondary pictures.</param>
/// <param name="Provider">Provider of the app.</param>
/// <param name="UseCases">Names of the app's use cases.</param>
/// <param name="Description">description of the app.</param>
/// <param name="Documents">documents assigned to offer</param>
/// <param name="Roles">Roles assigned to offer</param>
/// <param name="Languages">Languages that the app is available in.</param>
/// <param name="ProviderUri">Uri to provider's marketing presence.</param>
/// <param name="ContactEmail">Email address of the app's primary contact.</param>
/// <param name="ContactNumber">Phone number of the app's primary contact.</param>
/// <param name="LicenseType">License TypeId for offer.</param>
/// <param name="Price">Pricing information of the app.</param>
/// <param name="Tags">Tags assigned to application.</param>
/// <param name="PrivacyPolicies">Privacy policy assigned to app.</param>
/// <param name="OfferStatusId">OfferStatusId of the app.</param>
/// <param name="TechnicalUserProfile">TechnicalUserProfile of the User.</param>

public record InReviewAppDetails(
	Guid Id,
	string Title,
	Guid LeadPictureId,
	IEnumerable<Guid> Images,
	string Provider,
	IEnumerable<string> UseCases,
	IEnumerable<LocalizedDescription> Description,
	IDictionary<DocumentTypeId, IEnumerable<DocumentData>> Documents,
	IEnumerable<string> Roles,
	IEnumerable<string> Languages,
	string ProviderUri,
	string? ContactEmail,
	string? ContactNumber,
	LicenseTypeId LicenseType,
	string Price,
	IEnumerable<string> Tags,
	IEnumerable<PrivacyPolicyId> PrivacyPolicies,
	OfferStatusId OfferStatusId,
	IDictionary<Guid, IEnumerable<string>> TechnicalUserProfile
);

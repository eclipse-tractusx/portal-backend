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
/// View model of an application's Service detailed data.
/// </summary>
public record ServiceDetailsData(

	/// <summary>
	/// ID of the offer.
	/// </summary>
	Guid Id,

	/// <summary>
	/// Title or name of the offer.
	/// </summary>
	string? Title,

	/// <summary>
	/// Service Type Id of the offer
	/// </summary>
	IEnumerable<ServiceTypeId> ServiceTypeIds,

	/// <summary>
	/// Provider of the offer.
	/// </summary>
	string Provider,

	/// <summary>
	/// Descriptions of the offer.
	/// </summary>
	IEnumerable<LocalizedDescription> Descriptions,

	/// <summary>
	/// document assigned to offer
	/// </summary>
	IEnumerable<DocumentTypeData> Documents,

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
	/// Offer Status Id
	/// </summary>
	OfferStatusId OfferStatusId,

	/// <summary>
	/// License Type Id
	/// </summary>
	LicenseTypeId LicenseTypeId,

	///<summary>
	/// Technical User Profile
	///</summary>
	IEnumerable<TechnicalUserRoleData> TechnicalUserProfile
);

public record TechnicalUserRoleData(
	Guid TechnicalUserProfileId,
	IEnumerable<string> UserRoles
);

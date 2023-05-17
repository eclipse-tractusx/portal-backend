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
/// Model for Offer with status
/// </summary>
/// <param name="Title"></param>
/// <param name="Provider"></param>
/// <param name="LeadPictureId"></param>
/// <param name="ProviderName"></param>
/// <param name="UseCase"></param>
/// <param name="Descriptions"></param>
/// <param name="Agreements"></param>
/// <param name="SupportedLanguageCodes"></param>
/// <param name="Price"></param>
/// <param name="Images"></param>
/// <param name="ProviderUri"></param>
/// <param name="ContactEmail"></param>
/// <param name="ContactNumber"></param>
/// <param name="PrivacyPolicies"></param>
/// <param name="ServiceTypeIds"></param>
/// <returns></returns>
public record OfferProviderData(
	string? Title,
	string Provider,
	Guid LeadPictureId,
	string? ProviderName,
	IEnumerable<AppUseCaseData>? UseCase,
	IEnumerable<LocalizedDescription> Descriptions,
	IEnumerable<AgreementAssignedOfferData> Agreements,
	IEnumerable<string> SupportedLanguageCodes,
	string? Price,
	IEnumerable<Guid> Images,
	string? ProviderUri,
	string? ContactEmail,
	string? ContactNumber,
	IEnumerable<DocumentTypeData> Documents,
	Guid? SalesManagerId,
	IEnumerable<PrivacyPolicyId> PrivacyPolicies,
	IEnumerable<ServiceTypeId>? ServiceTypeIds,
	IEnumerable<TechnicalUserRoleData> TechnicalUserProfile
);

/// <summary>
/// 
/// </summary>
/// <param name="AgreementId"></param>
/// <param name="AgreementName"></param>
/// <returns></returns>
public record AgreementAssignedOfferData(
	Guid AgreementId,
	string? AgreementName,
	ConsentStatusId? ConsentStatusId
);

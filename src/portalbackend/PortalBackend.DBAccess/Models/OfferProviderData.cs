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
/// Model for Offer with status
/// </summary>
/// <param name="Title"></param>
/// <param name="Provider"></param>
/// <param name="LeadPictureUri"></param>
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
/// <returns></returns>
public record OfferProviderData(string? Title, string Provider, string? LeadPictureUri, string? ProviderName, IEnumerable<string> UseCase, IEnumerable<OfferDescriptionData> Descriptions, IEnumerable<OfferAgreement> Agreements, IEnumerable<string> SupportedLanguageCodes, string? Price, IEnumerable<string> Images, string? ProviderUri, string? ContactEmail, string? ContactNumber, IEnumerable<DocumentTypeData> Documents);

/// <summary>
/// Model for Offer Description
/// </summary>
/// <param name="languageCode"></param>
/// <param name="longDescription"></param>
/// <param name="shortDescription"></param>
/// <returns></returns>
public record OfferDescriptionData(string languageCode, string longDescription, string shortDescription);

/// <summary>
/// Model for Agreement and Consent Status
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="ConsentStatus"></param>
/// <returns></returns>
public record OfferAgreement(Guid? Id, string? Name, ConsentStatusId ConsentStatus);

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

namespace CatenaX.NetworkServices.Apps.Service.InputModels;

/// <summary>
/// Request Model for App Creation.
/// </summary>
/// <param name="Title">Title</param>
/// <param name="Provider">Provider</param>
/// <param name="LeadPictureUri">LeadPictureUri</param>
/// <param name="ProviderCompanyId">ProviderCompanyId</param>
/// <param name="UseCaseIds">UseCaseIds</param>
/// <param name="Descriptions">Descriptions</param>
/// <param name="SupportedLanguageCodes">SupportedLanguageCodes</param>
/// <param name="Price">Price</param>
/// <returns></returns>

public record AppRequestModel(string? Title, string Provider, string? LeadPictureUri, Guid? ProviderCompanyId, ICollection<string> UseCaseIds, ICollection<LocalizedDescription> Descriptions, ICollection<string> SupportedLanguageCodes, string Price);


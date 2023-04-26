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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing UseCase on persistence layer.
/// </summary>
public interface IStaticDataRepository
{
    /// <summary>
    /// Retrieves all Use Case.
    /// </summary>
    /// <returns>Returns a async enumerable of <see cref="UseCaseData"/></returns>
    IAsyncEnumerable<UseCaseData> GetAllUseCase();
    
    /// <summary>
    /// Retrieves all Language.
    /// </summary>
    /// <returns>Returns a async enumerable of <see cref="LanguageData"/></returns>
    IAsyncEnumerable<LanguageData> GetAllLanguage();
    
    /// <summary>
    /// Retrieve Unique Identifier Data for Country Alpha2Code
    /// </summary>
    /// <param name="alpha2Code"></param>
    /// <returns>Returns  enumerable of <see cref="UniqueIdentifierData"/> and IsCountryCodeExist</returns>
    Task<(IEnumerable<UniqueIdentifierId> IdentifierIds, bool IsValidCountryCode)> GetCompanyIdentifiers(string alpha2Code);

    Task<(bool IsValidCountry, IEnumerable<(BpdmIdentifierId BpdmIdentifierId, UniqueIdentifierId UniqueIdentifierId)> Identifiers)> GetCountryAssignedIdentifiers(IEnumerable<BpdmIdentifierId> bpdmIdentifierIds, string countryAlpha2Code);
    
    /// <summary>
    /// Retrieve Service Type Data
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<ServiceTypeData> GetServiceTypeData();
    
    /// <summary>
    /// Return all License Type Data
    /// </summary>
    /// <returns>Returns a async enumerable of <see cref="LicenseTypeData"/></returns>
    IAsyncEnumerable<LicenseTypeData> GetLicenseTypeData();
}

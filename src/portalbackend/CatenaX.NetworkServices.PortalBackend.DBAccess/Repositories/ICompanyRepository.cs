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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="Company"/> entities.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Creates new company entity from persistence layer.
    /// </summary>
    /// <param name="companyName">Name of the company to create the new entity for.</param>
    /// <returns>Created company entity.</returns>
    Company CreateCompany(string companyName);

    Address CreateAddress(string city, string streetname, string countryAlpha2Code);
    
    Task<(string? Name, Guid Id)> GetCompanyNameIdUntrackedAsync(string iamUserId);

    Task<CompanyNameIdIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId);

    /// <summary>
    /// Checks an set of CompanyIds for existence and returns the associated BusinessPartnerNumber if requested
    /// </summary>
    /// <param name="companyId">Id of the company to check</param>
    /// <param name="bpnRequested">whether the assigned businessPartnerNumber should be returned</param>
    /// <returns>(CompanyId, BusinessPartnerNumber) for any company that exists</returns>
    IAsyncEnumerable<(Guid CompanyId, string? BusinessPartnerNumber)> GetConnectorCreationCompanyDataAsync(IEnumerable<(Guid companyId, bool bpnRequested)> parameters);

    /// <summary>
    /// Get all member companies bpn
    /// </summary>
    /// <returns> Business partner numbers of all active companies</returns>
    IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync();
    Task<CompanyWithAddress?> GetOwnCompanyDetailsAsync(string iamUserId);

    /// <summary>
    /// Checks whether the iamUser is assigned to the company and the company exists
    /// </summary>
    /// <param name="serviceProviderCompanyId">Id of the company to check</param>
    /// <param name="iamUserId">IAm User Id</param>
    /// <param name="companyRole">The company Role</param>
    /// <returns><c>true</c> if the company exists for the given user, otherwise <c>false</c></returns>
    Task<bool> CheckCompanyIsServiceProviderAndExistsForIamUser(Guid serviceProviderCompanyId, string iamUserId, CompanyRoleId companyRole);

    /// <summary>
    /// Creates service provider company details
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="dataUrl">Url for the service provider</param>
    /// <returns>Returns the newly created entity</returns>
    ServiceProviderCompanyDetail CreateServiceProviderCompanyDetail(Guid companyId, string dataUrl);
}

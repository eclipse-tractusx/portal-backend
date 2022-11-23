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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    Company AttachAndModifyCompany(Guid companyId, Action<Company>? setOptionalParameters = null);

    Address CreateAddress(string city, string streetname, string countryAlpha2Code);
    
    Task<(string CompanyName, Guid CompanyId)> GetCompanyNameIdUntrackedAsync(string iamUserId);

    Task<(Guid CompanyId, string CompanyName, string? Alias, Guid CompanyUserId)> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId);

    /// <summary>
    /// Checks the bpn for existence and returns the associated CompanyId
    /// </summary>
    /// <param name="businessPartnerNumber">The business partner number</param>
    /// <returns>the company id or guid empty if not found</returns>
    Task<Guid> GetCompanyIdByBpnAsync(string businessPartnerNumber);

    /// <summary>
    /// Get all member companies bpn
    /// </summary>
    /// <returns> Business partner numbers of all active companies</returns>
    IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync();
    Task<CompanyWithAddress?> GetOwnCompanyDetailsAsync(string iamUserId);

    /// <summary>
    /// Checks whether the iamUser is assigned to the company and the company exists
    /// </summary>
    /// <param name="iamUserId">IAm User Id</param>
    /// <param name="companyRoleId">The company Role</param>
    /// <returns><c>true</c> if the company exists for the given user, otherwise <c>false</c></returns>
    Task<(Guid CompanyId, bool IsServiceProviderCompany)> GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(string iamUserId, CompanyRoleId companyRoleId);

    Task<(bool IsValidServicProviderDetailsId, bool IsSameCompany)> CheckProviderCompanyDetailsExistsForUser(string iamUserId, Guid providerCompanyDetailsId);
    
    /// <summary>
    /// Creates service provider company details
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="dataUrl">Url for the service provider</param>
    /// <returns>Returns the newly created entity</returns>
    ProviderCompanyDetail CreateProviderCompanyDetail(Guid companyId, string dataUrl);

    /// <summary>
    /// Gets the service provider company details data
    /// </summary>
    /// <param name="providerDetailDataId">Id of the details</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the details data</returns>
    Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany, bool IsCompanyUser)> GetProviderCompanyDetailAsync(Guid providerDetailDataId, CompanyRoleId companyRoleId, string iamUserId);
    
    /// <summary>
    /// Updates the service provider company details
    /// </summary>
    /// <param name="providerCompanyDetailId">Id of the service provider company details</param>
    /// <param name="setOptionalParameters">sets the fields that should be updated.</param>
    /// <returns></returns>
    ProviderCompanyDetail AttachAndModifyProviderCompanyDetails(Guid providerCompanyDetailId, Action<ProviderCompanyDetail>? setOptionalParameters = null);

    /// <summary>
    /// Gets the business partner number for the given id
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns the business partner number</returns>
    Task<string?> GetCompanyBpnByIdAsync(Guid companyId);

    /// <summary>
    /// Gets the bpdm data for the given application
    /// </summary>
    /// <param name="iamUserId">Id of the user</param>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Returns the bpdm data</returns>
    Task<BpdmData?> GetBpdmDataForApplicationAsync(string iamUserId, Guid applicationId);
}

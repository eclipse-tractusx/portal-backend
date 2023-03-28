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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    void AttachAndModifyCompany(Guid companyId, Action<Company>? initialize, Action<Company> modify);

    Address CreateAddress(string city, string streetname, string countryAlpha2Code, Action<Address>? setOptionalParameters = null);

    void AttachAndModifyAddress(Guid addressId, Action<Address>? initialize, Action<Address> modify);

    void CreateUpdateDeleteIdentifiers(Guid companyId, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initialItems, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modifiedItems);

    Task<(string CompanyName, Guid CompanyId)> GetCompanyNameIdUntrackedAsync(string iamUserId);

    /// <summary>
    /// Checks the bpn for existence and returns the associated CompanyId
    /// </summary>
    /// <param name="businessPartnerNumber">The business partner number</param>
    /// <returns>the company id or guid empty if not found</returns>
    Task<(Guid CompanyId, Guid? SelfDescriptionDocumentId)> GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(string businessPartnerNumber);

    /// <summary>
    /// Get all member companies bpn
    /// </summary>
    /// <returns> Business partner numbers of all active companies</returns>
    IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync();
    Task<CompanyAddressDetailData?> GetOwnCompanyDetailsAsync(string iamUserId);

    /// <summary>
    /// Checks whether the iamUser is assigned to the company and the company exists
    /// </summary>
    /// <param name="iamUserId">IAm User Id</param>
    /// <param name="companyRoleId">The company Role</param>
    /// <returns><c>true</c> if the company exists for the given user, otherwise <c>false</c></returns>
    Task<(Guid CompanyId, bool IsServiceProviderCompany)> GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(string iamUserId, CompanyRoleId companyRoleId);

    Task<(Guid ProviderCompanyDetailId, string Url)> GetProviderCompanyDetailsExistsForUser(string iamUserId);
    
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
    /// <param name="companyRoleId">Id of the details</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the details data</returns>
    Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany)> GetProviderCompanyDetailAsync(CompanyRoleId companyRoleId, string iamUserId);
    
    /// <summary>
    /// Updates the service provider company details
    /// </summary>
    /// <param name="providerCompanyDetailId">Id of the service provider company details</param>
    /// <param name="initialize">sets the fields that should be initialized.</param>
    /// <param name="modify">sets the fields that should be updated.</param>
    /// <returns></returns>
    void AttachAndModifyProviderCompanyDetails(Guid providerCompanyDetailId, Action<ProviderCompanyDetail> initialize, Action<ProviderCompanyDetail> modify);

    /// <summary>
    /// Gets the business partner number for the given id
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns the business partner number</returns>
    Task<(string? Bpn, Guid? SelfDescriptionDocumentId)> GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(Guid companyId);

    /// <summary>
    /// Gets the the companyAssigendUeseCase Details
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the companyAssigendUeseCase Details</returns>
    IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(string iamUserId);

    /// <summary>
    /// Gets the CompanyActive Status and companyAssigendUeseCase Id
    /// </summary>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="useCaseId">Id of the UseCase</param>
    /// <returns>Returns the CompanyActive Status, companyAssigendUeseCase Id and CompanyId</returns>
    Task<(bool isUseCaseIdExists, bool isActiveCompanyStatus , Guid companyId)> GetCompanyStatusAndUseCaseIdAsync(string iamUserId, Guid useCaseId);

    /// <summary>
    /// creates the companyAssigendUeseCase record
    /// </summary>
    /// <param name="companyId">Id of the comapny</param>
    /// <param name="useCaseId">Id of the UseCase</param>
    CompanyAssignedUseCase CreateCompanyAssignedUseCase(Guid companyId, Guid useCaseId);

    /// <summary>
    /// Remove the companyAssigendUeseCase record
    /// </summary>
    /// <param name="companyId">Id of the comapny</param>
    /// <param name="useCaseId">Id of the UseCase</param>
    void RemoveCompanyAssignedUseCase(Guid companyId, Guid useCaseId);
}

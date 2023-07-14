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

    Task<(bool IsValidCompany, string CompanyName)> GetCompanyNameUntrackedAsync(Guid companyId);

    Task<(string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetBpnAndTechnicalUserRoleIds(Guid companyId, string technicalUserClientId);

    /// <summary>
    /// Get all member companies bpn
    /// </summary>
    /// <returns> Business partner numbers of all active companies</returns>
    IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync();
    Task<CompanyAddressDetailData?> GetCompanyDetailsAsync(Guid companyId);

    /// <summary>
    /// Checks whether the iamUser is assigned to the company and the company exists
    /// </summary>
    /// <param name="companyId">Id of the users company</param>
    /// <param name="companyRoleIds">The company Roles</param>
    /// <returns><c>true</c> if the company exists for the given user, otherwise <c>false</c></returns>
    Task<(bool IsValidCompanyId, bool IsCompanyRoleOwner)> IsValidCompanyRoleOwner(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds);

    Task<(Guid ProviderCompanyDetailId, string Url)> GetProviderCompanyDetailsExistsForUser(Guid companyId);

    /// <summary>
    /// Creates service provider company details
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="dataUrl">Url for the service provider</param>
    /// <param name="setOptionalParameter">action to set optional parameter</param>
    /// <returns>Returns the newly created entity</returns>
    ProviderCompanyDetail CreateProviderCompanyDetail(Guid companyId, string dataUrl, Action<ProviderCompanyDetail>? setOptionalParameter = null);

    /// <summary>
    /// Gets the service provider company details data
    /// </summary>
    /// <param name="companyRoleId">Id of the details</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details data</returns>
    Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany)> GetProviderCompanyDetailAsync(CompanyRoleId companyRoleId, Guid companyId);

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
    /// <param name="userCompanyId">Id of the iam users company</param>
    /// <returns>Returns the companyAssigendUeseCase Details</returns>
    IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(Guid userCompanyId);

    /// <summary>
    /// Gets the CompanyActive Status and companyAssigendUeseCase Id
    /// </summary>
    /// <param name="companyId">Id of the users company</param>
    /// <param name="useCaseId">Id of the UseCase</param>
    /// <returns>Returns the CompanyActive Status, companyAssigendUeseCase Id and CompanyId</returns>
    Task<(bool IsUseCaseIdExists, bool IsActiveCompanyStatus, bool IsValidCompany)> GetCompanyStatusAndUseCaseIdAsync(Guid companyId, Guid useCaseId);

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

    /// <summary>
    /// Gets the the companyRole and ConsentAgreemnet
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="languageShortName"></param>
    /// <returns>Returns the companyRole and ConsentAgreemnet</returns>
    IAsyncEnumerable<CompanyRoleConsentData> GetCompanyRoleAndConsentAgreementDataAsync(Guid companyId, string languageShortName);

    /// <summary>
    /// Gets the the companyRole
    /// </summary>
    /// <param name="companyId">Id of the companyr</param>
    /// <returns>Returns the companyRole</returns>
    Task<(bool IsValidCompany, bool IsCompanyActive, IEnumerable<CompanyRoleId>? CompanyRoleIds, IEnumerable<ConsentStatusDetails>? ConsentStatusDetails)> GetCompanyRolesDataAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds);

    /// <summary>
    /// Gets the the AgreementAssignedCompanyRoles Data
    /// </summary>
    /// <param name="companyRoleIds">Id of the CompanyRole</param>
    /// <returns>Returns the AgreementAssignedCompanyRoles Data</returns>
    IAsyncEnumerable<(Guid AgreementId, CompanyRoleId CompanyRoleId)> GetAgreementAssignedRolesDataAsync(IEnumerable<CompanyRoleId> companyRoleIds);

    /// <summary>
    /// Gets the the CompanyStatus Data
    /// </summary>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the CompanyStatus Data</returns>
    Task<(bool IsActive, bool IsValid)> GetCompanyStatusDataAsync(Guid companyId);

    Task<CompanyInformationData?> GetOwnCompanyInformationAsync(Guid companyId);

    /// <summary>
    /// Gets all bpns of companies with role operator
    /// </summary>
    /// <returns>Async enumerable of bpns</returns>
    IAsyncEnumerable<OperatorBpnData> GetOperatorBpns();
}

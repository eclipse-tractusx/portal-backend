/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

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
    /// <param name="setOptionalParameters">Sets the optional Parameters</param>
    /// <returns>Created company entity.</returns>
    Company CreateCompany(string companyName, Action<Company>? setOptionalParameters = null);

    void AttachAndModifyCompany(Guid companyId, Action<Company>? initialize, Action<Company> modify);

    Address CreateAddress(string city, string streetname, string region, string countryAlpha2Code, Action<Address>? setOptionalParameters = null);

    void AttachAndModifyAddress(Guid addressId, Action<Address>? initialize, Action<Address> modify);

    void CreateUpdateDeleteIdentifiers(Guid companyId, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initialItems, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modifiedItems);

    Task<(string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetBpnAndTechnicalUserRoleIds(Guid companyId, string technicalUserClientId);

    /// <summary>
    /// Get all member companies bpn
    /// </summary>
    /// <param name="bpnIds">Id of the users company</param>
    /// <returns> Business partner numbers of all active companies</returns>
    IAsyncEnumerable<string> GetAllMemberCompaniesBPNAsync(IEnumerable<string>? bpnIds);
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
    IAsyncEnumerable<(AgreementStatusData agreementStatusData, CompanyRoleId CompanyRoleId)> GetAgreementAssignedRolesDataAsync(IEnumerable<CompanyRoleId> companyRoleIds);

    /// <summary>
    /// Gets the the CompanyStatus Data
    /// </summary>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the CompanyStatus Data</returns>
    Task<(bool IsActive, bool IsValid)> GetCompanyStatusDataAsync(Guid companyId);

    Task<CompanyInformationData?> GetOwnCompanyInformationAsync(Guid companyId, Guid companyUserId);
    IAsyncEnumerable<CompanyRoleId> GetOwnCompanyRolesAsync(Guid companyId);

    /// <summary>
    /// Gets all bpns of companies with role operator
    /// </summary>
    /// <returns>Async enumerable of bpns</returns>
    IAsyncEnumerable<OperatorBpnData> GetOperatorBpns();

    Task<(bool IsValidCompany, string CompanyName, bool IsAllowed)> CheckCompanyAndCompanyRolesAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoles);
    Task<OnboardingServiceProviderCallbackResponseData> GetCallbackData(Guid companyId);
    Task<(bool HasCompanyRole, Guid? OnboardingServiceProviderDetailId, OspDetails? OspDetails)> GetCallbackEditData(Guid companyId, CompanyRoleId companyRoleId);
    void AttachAndModifyOnboardingServiceProvider(Guid onboardingServiceProviderDetailId, Action<OnboardingServiceProviderDetail>? initialize, Action<OnboardingServiceProviderDetail> setOptionalFields);
    OnboardingServiceProviderDetail CreateOnboardingServiceProviderDetails(Guid companyId, string callbackUrl, string authUrl, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode);
    Task<bool> CheckBpnExists(string bpn);
    void CreateWalletData(Guid companyId, string did, JsonDocument didDocument, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode, string authenticationServiceUrl);
    Task<(bool Exists, JsonDocument DidDocument)> GetDidDocumentById(string bpn);
    IAsyncEnumerable<(Guid CompanyId, IEnumerable<Guid> SubmittedApplicationIds)> GetCompanySubmittedApplicationIdsByBpn(string bpn);
    Task<(string? Bpn, string? Did, string? WalletUrl)> GetDimServiceUrls(Guid companyId);
    Task<(string? Holder, string? BusinessPartnerNumber, WalletInformation? WalletInformation)> GetWalletData(Guid identityId);
    void RemoveProviderCompanyDetails(Guid providerCompanyDetailId);
    Func<int, int, Task<Pagination.Source<CompanyMissingSdDocumentData>?>> GetCompaniesWithMissingSdDocument();
    IAsyncEnumerable<Guid> GetCompanyIdsWithMissingSelfDescription();
    Task<(Guid Id, string LegalName, IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers, string? BusinessPartnerNumber, string? CountryCode, string? Region)> GetCompanyByProcessId(Guid processId);
    Task<bool> IsExistingCompany(Guid companyId);
    Task<(bool Exists, Guid CompanyId, IEnumerable<Guid> SubmittedCompanyApplicationId)> GetCompanyIdByBpn(string bpn);
    Task<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?> GetProcessDataForCompanyIdId(Guid companyId);
}

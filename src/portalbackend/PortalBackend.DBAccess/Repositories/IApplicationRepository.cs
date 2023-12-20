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

public interface IApplicationRepository
{
    CompanyApplication CreateCompanyApplication(Guid companyId, CompanyApplicationStatusId companyApplicationStatusId, CompanyApplicationTypeId applicationTypeId, Action<CompanyApplication>? setOptionalFields = null);
    void AttachAndModifyCompanyApplication(Guid companyApplicationId, Action<CompanyApplication> setOptionalParameters);
    void AttachAndModifyCompanyApplications(IEnumerable<(Guid companyApplicationId, Action<CompanyApplication>? Initialize, Action<CompanyApplication> Modify)> applicationData);
    Invitation CreateInvitation(Guid applicationId, Guid companyUserId);
    void DeleteInvitations(IEnumerable<Guid> invitationIds);
    Task<(bool Exists, CompanyApplicationStatusId StatusId)> GetOwnCompanyApplicationUserDataAsync(Guid applicationId, Guid userCompanyId);
    Task<(bool Exists, bool IsUserOfCompany, CompanyApplicationStatusId ApplicationStatus)> GetOwnCompanyApplicationStatusUserDataUntrackedAsync(Guid applicationId, Guid companyId);
    Task<CompanyApplicationUserEmailData?> GetOwnCompanyApplicationUserEmailDataAsync(Guid applicationId, Guid companyUserId, IEnumerable<DocumentTypeId> submitDocumentTypeIds);
    IQueryable<CompanyApplication> GetCompanyApplicationsFilteredQuery(string? companyName = null, IEnumerable<CompanyApplicationStatusId>? applicationStatusIds = null);
    Task<CompanyApplicationDetailData?> GetCompanyApplicationDetailDataAsync(Guid applicationId, Guid userCompanyId, Guid? companyId = null);
    Task<(string CompanyName, string? FirstName, string? LastName, string? Email, IEnumerable<(Guid ApplicationId, CompanyApplicationStatusId ApplicationStatusId, IEnumerable<(string? FirstName, string? LastName, string? Email)> InvitedUsers)> Applications)> GetCompanyApplicationsDeclineData(Guid companyUserId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds);
    Task<(bool IsValidApplicationId, Guid CompanyId, bool IsSubmitted)> GetCompanyIdSubmissionStatusForApplication(Guid applicationId);
    Task<(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber, IEnumerable<string> IamIdpAliasse, CompanyApplicationTypeId ApplicationTypeId, Guid? NetworkRegistrationProcessId)> GetCompanyAndApplicationDetailsForApprovalAsync(Guid applicationId);
    Task<(Guid CompanyId, string CompanyName, string? BusinessPartnerNumber)> GetCompanyAndApplicationDetailsForCreateWalletAsync(Guid applicationId);
    IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<EmailData> GetEmailDataUntrackedAsync(Guid applicationId);
    IQueryable<CompanyApplication> GetAllCompanyApplicationsDetailsQuery(string? companyName = null);
    Task<CompanyUserRoleWithAddress?> GetCompanyUserRoleWithAddressUntrackedAsync(Guid companyApplicationId);
    Task<(bool IsValidApplicationId, bool IsValidCompany, RegistrationData? Data)> GetRegistrationDataUntrackedAsync(Guid applicationId, Guid userCompanyId, IEnumerable<DocumentTypeId> documentTypes);
    Task<(string? Bpn, IEnumerable<ApplicationChecklistEntryTypeId> ExistingChecklistEntryTypeIds)> GetBpnAndChecklistCheckForApplicationIdAsync(Guid applicationId);

    Task<(Guid CompanyId, string? BusinessPartnerNumber, string Alpha2Code, IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers)> GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(Guid applicationId);

    /// <summary>
    /// Gets the application status and the status of the application checklist entry of the given type
    /// </summary>
    /// <param name="applicationId">Id of the application to get the states for</param>
    /// <param name="checklistEntryTypeId">Type of the checklist entry to check the status for</param>
    /// <returns>Returns the status of the application checklist entry of the given type</returns>
    Task<(CompanyApplicationStatusId ApplicationStatusId, ApplicationChecklistEntryStatusId RegistrationVerificationStatusId)> GetApplicationStatusWithChecklistTypeStatusAsync(Guid applicationId, ApplicationChecklistEntryTypeId checklistEntryTypeId);

    /// <summary>
    /// Gets the company bpn for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Returns the bpn</returns>
    Task<string?> GetBpnForApplicationIdAsync(Guid applicationId);

    /// <summary>
    /// Gets the data needed to make the clearinghouse request for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Return the data needed to make the clearinghouse request</returns>
    Task<ClearinghouseData?> GetClearinghouseDataForApplicationId(Guid applicationId);

    /// <summary>
    /// Gets the submitted application id and the clearinghouse checklist state for the given bpn
    /// </summary>
    /// <param name="bpn">Bpn of the company to get the application for</param>
    /// <returns></returns>
    IAsyncEnumerable<Guid> GetSubmittedApplicationIdsByBpn(string bpn);

    /// <summary>
    /// Gets the bpdm data for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Returns the bpdm data</returns>
    Task<(Guid CompanyId, BpdmData BpdmData)> GetBpdmDataForApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets the checklist data for a specific application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>Returns the checklist data</returns>
    Task<(bool Exists, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId, string? Comment)> ChecklistData, IEnumerable<ProcessStepTypeId> ProcessStepTypeIds)> GetApplicationChecklistData(Guid applicationId, IEnumerable<ProcessStepTypeId> processStepTypeIds);

    /// <summary>
    /// Gets the id of the company for the submitted application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <returns>The id of the company for the given application</returns>
    Task<(Guid CompanyId, string CompanyName, Guid? NetworkRegistrationProcessId, IEnumerable<(Guid IdentityProviderId, string IamAlias, IdentityProviderTypeId TypeId)> Idps, IEnumerable<Guid> CompanyUserIds)> GetCompanyIdNameForSubmittedApplication(Guid applicationId);

    Task<bool> IsValidApplicationForCompany(Guid applicationId, Guid companyId);
}

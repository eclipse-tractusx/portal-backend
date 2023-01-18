/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
    CompanyApplication CreateCompanyApplication(Guid companyId, CompanyApplicationStatusId companyApplicationStatusId);
    void AttachAndModifyCompanyApplication(Guid companyApplicationId, Action<CompanyApplication> setOptionalParameters);
    Invitation CreateInvitation(Guid applicationId, Guid companyUserId);
    void DeleteInvitations(IEnumerable<Guid> invitationIds);
    Task<CompanyApplicationUserData?> GetOwnCompanyApplicationUserDataAsync(Guid applicationId, string iamUserId);
    Task<CompanyApplicationStatusUserData?> GetOwnCompanyApplicationStatusUserDataUntrackedAsync(Guid applicationId, string iamUserId);
    Task<CompanyApplicationUserEmailData?> GetOwnCompanyApplicationUserEmailDataAsync(Guid applicationId, string iamUserId);
    IQueryable<CompanyApplication> GetCompanyApplicationsFilteredQuery(string? companyName = null, IEnumerable<CompanyApplicationStatusId>? applicationStatusIds = null);
    Task<CompanyApplicationDetailData?> GetCompanyApplicationDetailDataAsync (Guid applicationId, string iamUserId, Guid? companyId = null);
    Task<CompanyApplication?> GetCompanyAndApplicationForSubmittedApplication(Guid applicationId);
    Task<(Guid companyId, string companyName, string? businessPartnerNumber, string countryCode)> GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(Guid applicationId);
    IAsyncEnumerable<CompanyInvitedUserData> GetInvitedUsersDataByApplicationIdUntrackedAsync(Guid applicationId);
    IAsyncEnumerable<WelcomeEmailData> GetWelcomeEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds);
    IAsyncEnumerable<WelcomeEmailData> GetRegistrationDeclineEmailDataUntrackedAsync(Guid applicationId, IEnumerable<Guid> roleIds);
    IQueryable<CompanyApplication> GetAllCompanyApplicationsDetailsQuery(string? companyName = null);
    Task<CompanyUserRoleWithAddress?> GetCompanyUserRoleWithAdressUntrackedAsync(Guid companyApplicationId);
    Task<(bool IsValidApplicationId, bool IsSameCompanyUser, RegistrationData? Data)> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId, IEnumerable<DocumentTypeId> documentTypes);
    Task<(string? Bpn, bool ChecklistAlreadyExists)> GetBpnAndChecklistCheckForApplicationIdAsync(Guid applicationId);

    /// <summary>
    /// Gets the application status and the status of the application checklist entry of the given type
    /// </summary>
    /// <param name="applicationId">Id of the application to get the states for</param>
    /// <param name="checklistEntryTypeId">Type of the checklist entry to check the status for</param>
    /// <returns>Returns the status of the application checklist entry of the given type</returns>
    Task<(CompanyApplicationStatusId ApplicationStatusId, ApplicationChecklistEntryStatusId RegistrationVerificationStatusId)> GetApplicationStatusWithChecklistTypeStatusAsync(Guid applicationId, ApplicationChecklistEntryTypeId checklistEntryTypeId);
}

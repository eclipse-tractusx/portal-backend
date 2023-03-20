/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic
{
    public interface IRegistrationBusinessLogic
    {
        [Obsolete($"use {nameof(GetCompanyBpdmDetailDataByBusinessPartnerNumber)} instead")]
        IAsyncEnumerable<FetchBusinessPartnerDto> GetCompanyByIdentifierAsync(string companyIdentifier, string token, CancellationToken cancellationToken);
        Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumber(string businessPartnerNumber, string token, CancellationToken cancellationToken);
        IAsyncEnumerable<string> GetClientRolesCompositeAsync();
        Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, string iamUserId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Gets the file content from the persistence store for the given user
        /// </summary>
        /// <param name="documentId">The Id of the document that should be get</param>
        /// <param name="iamUserId">The Id of the current user</param>
        /// <returns></returns>
        Task<(string FileName, byte[] Content, string MediaType)> GetDocumentContentAsync(Guid documentId, string iamUserId);

        IAsyncEnumerable<CompanyApplicationData> GetAllApplicationsForUserWithStatus(string userId);
        Task<CompanyDetailData> GetCompanyDetailData(Guid applicationId, string iamUserId);
        Task SetCompanyDetailDataAsync(Guid applicationId, CompanyDetailData companyDetails, string iamUserId);
        Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, string iamUserId);
        Task<int> SetOwnCompanyApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status, string iamUserId);
        Task<CompanyApplicationStatusId> GetOwnCompanyApplicationStatusAsync(Guid applicationId, string iamUserId);
        Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, string iamUserId);
        Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, string iamUserId);
        Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync();
        Task<bool> SubmitRegistrationAsync(Guid applicationId, string iamUserId);
        IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId);
        Task<IEnumerable<UploadDocuments>> GetUploadedDocumentsAsync(Guid applicationId,DocumentTypeId documentTypeId,string iamUserId);
        Task<int> SetInvitationStatusAsync(string iamUserId);
        Task<CompanyRegistrationData> GetRegistrationDataAsync(Guid applicationId, string iamUserId);
        Task<bool> DeleteRegistrationDocumentAsync(Guid documentId, string iamUserId);
        IAsyncEnumerable<CompanyRolesDetails> GetCompanyRoles(string? languageShortName = null);
        Task<IEnumerable<UniqueIdentifierData>> GetCompanyIdentifiers(string alpha2Code);
    }
}

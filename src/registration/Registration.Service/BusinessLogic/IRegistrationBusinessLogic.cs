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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic
{
    public interface IRegistrationBusinessLogic
    {
        [Obsolete($"use {nameof(GetCompanyBpdmDetailDataByBusinessPartnerNumber)} instead")]
        IAsyncEnumerable<FetchBusinessPartnerDto> GetCompanyByIdentifierAsync(string companyIdentifier, string token, CancellationToken cancellationToken);
        Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumber(string businessPartnerNumber, string token, CancellationToken cancellationToken);
        IAsyncEnumerable<string> GetClientRolesCompositeAsync();
        Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file content from the persistence store for the given user
        /// </summary>
        /// <param name="documentId">The Id of the document that should be get</param>
        /// <param name="userId">The Identity of the current user</param>
        /// <returns></returns>
        Task<(string FileName, byte[] Content, string MediaType)> GetDocumentContentAsync(Guid documentId, Guid userId);

        IAsyncEnumerable<CompanyApplicationData> GetAllApplicationsForUserWithStatus(Guid companyId);
        Task<CompanyDetailData> GetCompanyDetailData(Guid applicationId, Guid companyId);
        Task SetCompanyDetailDataAsync(Guid applicationId, CompanyDetailData companyDetails, Guid companyId);
        Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, Guid userId);
        Task<int> SetOwnCompanyApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status, Guid companyId);
        Task<CompanyApplicationStatusId> GetOwnCompanyApplicationStatusAsync(Guid applicationId, Guid companyId);
        Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, Guid userId, Guid companyId);
        Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, Guid userId);
        Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync();
        Task<bool> SubmitRegistrationAsync(Guid applicationId, Guid userId);
        IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId);
        Task<IEnumerable<UploadDocuments>> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, Guid userId);
        Task<int> SetInvitationStatusAsync(Guid userId);
        Task<CompanyRegistrationData> GetRegistrationDataAsync(Guid applicationId, Guid companyId);
        Task<bool> DeleteRegistrationDocumentAsync(Guid documentId, Guid companyId);
        IAsyncEnumerable<CompanyRolesDetails> GetCompanyRoles(string? languageShortName = null);
        Task<IEnumerable<UniqueIdentifierData>> GetCompanyIdentifiers(string alpha2Code);
        Task<(string fileName, byte[] content, string mediaType)> GetRegistrationDocumentAsync(Guid documentId);
    }
}

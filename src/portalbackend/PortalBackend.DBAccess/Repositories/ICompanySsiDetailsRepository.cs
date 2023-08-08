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

public interface ICompanySsiDetailsRepository
{
    /// <summary>
    /// Gets the company credential details for the given company id
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="language">language short code for the use case name</param>
    /// <returns>AsyncEnumerable of UseCaseParticipation</returns>
    IAsyncEnumerable<UseCaseParticipationTransferData> GetUseCaseParticipationForCompany(Guid companyId, string language);

    /// <summary>
    /// Gets the company credential details for the given company id
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>AsyncEnumerable of SsiCertificateData</returns>
    IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(Guid companyId);

    /// <summary>
    /// Creates the credential details
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="verifiedCredentialTypeId">Id of the credential types</param>
    /// <param name="docId">id of the document</param>
    /// <param name="companySsiDetailStatusId">id of detail status</param>
    /// <param name="userId">Id of the creator</param>
    /// <param name="setOptionalFields">sets the optional fields</param>
    /// <returns>The created entity</returns>
    CompanySsiDetail CreateSsiDetails(Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, Guid docId, CompanySsiDetailStatusId companySsiDetailStatusId, Guid userId, Action<CompanySsiDetail>? setOptionalFields);

    /// <summary>
    /// Checks whether the credential details are already exists for the company and the given version
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="verifiedCredentialTypeId">Id of the verifiedCredentialType</param>
    /// <param name="kindId">Id of the credentialTypeKind</param>
    /// <param name="verifiedCredentialExternalTypeUseCaseDetailId">Id of the verifiedCredentialExternalType Detail Id</param>
    /// <returns><c>true</c> if the details already exists, otherwise <c>false</c></returns>
    Task<bool> CheckSsiDetailsExistsForCompany(Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? verifiedCredentialExternalTypeUseCaseDetailId);

    /// <summary>
    /// Checks whether the given externalTypeDetail exists and returns the CredentialTypeId
    /// </summary>
    /// <param name="verifiedCredentialExternalTypeUseCaseDetailId">Id of vc external type use case detail id</param>
    /// <param name="verifiedCredentialTypeId">Id of the vc type</param>
    /// <returns>Returns a valueTuple with identifiers if the externalTypeUseCaseDetailId exists and the corresponding credentialTypeId</returns>
    Task<bool> CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(Guid verifiedCredentialExternalTypeUseCaseDetailId, VerifiedCredentialTypeId verifiedCredentialTypeId);

    /// <summary>
    /// Checks whether the given credentialTypeId is a <see cref="VerifiedCredentialTypeKindId"/> Certificate
    /// </summary>
    /// <param name="credentialTypeId">Id of the credentialTypeId</param>
    /// <returns><c>true</c> if the tpye is a certificate, otherwise <c>false</c></returns>
    Task<bool> CheckSsiCertificateType(VerifiedCredentialTypeId credentialTypeId);

    /// <summary>
    /// Gets all credential details
    /// </summary>
    /// <param name="companySsiDetailStatusId">The status of the details</param>
    /// <param name="credentialTypeId">OPTIONAL: The type of the credential that should be returned</param>
    /// <param name="companyName">OPTIONAL: Search string for the company name</param>
    /// <returns>Returns data to create the pagination</returns>
    IQueryable<CompanySsiDetail> GetAllCredentialDetails(CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, string? companyName);

    Task<(bool exists, SsiApprovalData data)> GetSsiApprovalData(Guid credentialId);
    Task<(bool Exists, CompanySsiDetailStatusId Status, VerifiedCredentialTypeId Type, Guid RequesterId, string? RequesterEmail, string? Firstname, string? Lastname)> GetSsiRejectionData(Guid credentialId);
    void AttachAndModifyCompanySsiDetails(Guid id, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields);
    IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes(Guid companyId);
}

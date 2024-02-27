/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ICompanyCertificateRepository
{
    /// <summary>
    /// Checks whether the given CertificateType is a <see cref="CompanyCertificateTypeId"/> Certificate
    /// </summary>
    /// <param name="certificateTypeId">Id of the credentialTypeId</param>
    /// <returns><c>true</c> if the tpye is a certificate, otherwise <c>false</c></returns>
    Task<bool> CheckCompanyCertificateType(CompanyCertificateTypeId certificateTypeId);

    /// <summary>
    /// Creates the company certificate data
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <param name="companyCertificateTypeId">Id of the company certificate types</param>
    /// <param name="docId">id of the document</param>
    /// <param name="setOptionalFields">Action to set optional fields</param>   
    /// <returns>The created entity</returns>
    CompanyCertificate CreateCompanyCertificate(Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, Guid docId, Action<CompanyCertificate>? setOptionalFields = null);

    /// <summary>
    /// Get companyId against businessPartnerNumber
    /// </summary>
    /// <param name="businessPartnerNumber">bpn Id</param>
    /// <returns>company entity</returns>
    Task<Guid> GetCompanyIdByBpn(string businessPartnerNumber);

    /// <summary>
    /// Gets company certificate details
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns the CompanyCertificateBpnData Details</returns>
    IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificateData(Guid companyId);

    /// <summary>
    /// Gets all company certificate data from the persistence storage as pagination 
    /// </summary>
    /// <returns>Returns an Pagination</returns>
    Func<int, int, Task<Pagination.Source<CompanyCertificateData>?>> GetActiveCompanyCertificatePaginationSource(CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType, Guid companyId);

    Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, Guid CompanyCertificateId, bool IsSameCompany)> GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(Guid documentId, Guid companyUserId);

    void AttachAndModifyCompanyCertificateDetails(Guid id, Action<CompanyCertificate>? initialize, Action<CompanyCertificate> updateFields);

    void AttachAndModifyCompanyCertificateDocumentDetails(Guid id, Action<Document>? initialize, Action<Document> updateFields);
}

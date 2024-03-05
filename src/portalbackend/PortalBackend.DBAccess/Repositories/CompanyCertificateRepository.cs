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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyCertificateRepository : ICompanyCertificateRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanyCertificateRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    public Task<bool> CheckCompanyCertificateType(CompanyCertificateTypeId certificateTypeId) =>
     _context.CompanyCertificateTypes
            .AnyAsync(x =>
                x.CompanyCertificateTypeAssignedStatus!.CompanyCertificateTypeStatusId == CompanyCertificateTypeStatusId.ACTIVE &&
                x.Id == certificateTypeId);

    /// <inheritdoc />
    public CompanyCertificate CreateCompanyCertificate(Guid companyId, CompanyCertificateTypeId companyCertificateTypeId, Guid docId, Action<CompanyCertificate>? setOptionalFields)
    {
        var companyCertificate = new CompanyCertificate(Guid.NewGuid(), DateTimeOffset.UtcNow, companyCertificateTypeId, CompanyCertificateStatusId.ACTIVE, companyId, docId);
        setOptionalFields?.Invoke(companyCertificate);
        return _context.CompanyCertificates.Add(companyCertificate).Entity;
    }

    /// <inheritdoc />
    public Task<Guid> GetCompanyIdByBpn(string businessPartnerNumber) =>
     _context.Companies
         .Where(x => x.BusinessPartnerNumber == businessPartnerNumber)
         .Select(x => x.Id)
         .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificateData(Guid companyId) =>
        _context.CompanyCertificates
        .Where(x => x.CompanyId == companyId && x.CompanyCertificateStatusId == CompanyCertificateStatusId.ACTIVE)
        .Select(ccb => new CompanyCertificateBpnData(
            ccb.CompanyCertificateTypeId,
            ccb.CompanyCertificateStatusId,
            ccb.DocumentId,
            ccb.ValidFrom,
            ccb.ValidTill))
        .ToAsyncEnumerable();

    public Func<int, int, Task<Pagination.Source<CompanyCertificateData>?>> GetActiveCompanyCertificatePaginationSource(CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType, Guid companyId) =>
          (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.CompanyCertificates
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyId &&
                    (certificateStatus == null || x.CompanyCertificateStatusId == certificateStatus) &&
                    (certificateType == null || x.CompanyCertificateTypeId == certificateType))
                .GroupBy(x => x.CompanyId),
            sorting switch
            {
                CertificateSorting.CertificateTypeAsc => (IEnumerable<CompanyCertificate> cc) => cc.OrderBy(x => x.CompanyCertificateTypeId),
                CertificateSorting.CertificateTypeDesc => (IEnumerable<CompanyCertificate> cc) => cc.OrderByDescending(x => x.CompanyCertificateTypeId),
                CertificateSorting.ExpiryDateAsc => (IEnumerable<CompanyCertificate> cc) => cc.OrderBy(x => x.ValidTill),
                CertificateSorting.ExpiryDateDesc => (IEnumerable<CompanyCertificate> cc) => cc.OrderByDescending(x => x.ValidTill),
                _ => null
            },
            companyCertificate => new CompanyCertificateData(
                companyCertificate.CompanyCertificateTypeId,
                companyCertificate.CompanyCertificateStatusId,
                companyCertificate.DocumentId,
                companyCertificate.ValidFrom,
                companyCertificate.ValidTill
                ))
        .SingleOrDefaultAsync();

    public Task<(byte[] Content, string FileName, MediaTypeId MediaTypeId, bool Exists, bool IsStatusLocked)> GetCompanyCertificateDocumentDataAsync(Guid documentId, DocumentTypeId documentTypeId) =>
            _context.Documents
            .Where(x => x.Id == documentId &&
                   x.DocumentTypeId == documentTypeId)
            .Select(x => new ValueTuple<byte[], string, MediaTypeId, bool, bool>(x.DocumentContent, x.DocumentName, x.MediaTypeId, true, x.DocumentStatusId == DocumentStatusId.LOCKED))
        .SingleOrDefaultAsync();

    public Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> CompanyCertificateId, bool IsSameCompany)> GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(Guid documentId, Guid companyId) =>
            _context.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(document => new ValueTuple<Guid, DocumentStatusId, IEnumerable<Guid>, bool>(
                    document.Id,
                    document.DocumentStatusId,
                    document.CompanyCertificates.Where(x => x.CompanyCertificateStatusId != CompanyCertificateStatusId.INACTIVE).Select(x => x.Id),
                    document.CompanyUser!.Identity!.CompanyId == companyId))
            .SingleOrDefaultAsync();

    public void AttachAndModifyCompanyCertificateDetails(Guid id, Action<CompanyCertificate>? initialize, Action<CompanyCertificate> updateFields)
    {
        var entity = new CompanyCertificate(id, default, default, default, default, default);
        initialize?.Invoke(entity);
        _context.Attach(entity);
        updateFields.Invoke(entity);
    }

    public void AttachAndModifyCompanyCertificateDocumentDetails(Guid id, Action<Document>? initialize, Action<Document> updateFields)
    {
        var entity = new Document(id, null!, null!, null!, default, default, default, default);
        initialize?.Invoke(entity);
        _context.Attach(entity);
        updateFields.Invoke(entity);
    }

    public Task<(byte[] Content, string FileName, MediaTypeId MediaTypeId, bool Exists)> GetCompanyCertificateDocumentByCompanyIdDataAsync(Guid documentId, Guid companyId, DocumentTypeId documentTypeId) =>
        _context.Documents
        .Where(x => x.Id == documentId &&
               x.DocumentTypeId == documentTypeId &&
               x.CompanyUser!.Identity!.CompanyId == companyId)
        .Select(x => new ValueTuple<byte[], string, MediaTypeId, bool>(x.DocumentContent, x.DocumentName, x.MediaTypeId, true))
    .SingleOrDefaultAsync();
}

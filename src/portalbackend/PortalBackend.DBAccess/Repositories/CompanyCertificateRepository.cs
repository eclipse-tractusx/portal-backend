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

    public Func<int, int, Task<Pagination.Source<CompanyCertificateData>?>> GetActiveCompanyCertificatePaginationSource(CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType, Guid companyId) =>
          (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _context.CompanyCertificates
                .AsNoTracking()
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.CompanyCertificateStatusId == CompanyCertificateStatusId.ACTIVE &&
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
}

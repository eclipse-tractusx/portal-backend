/********************************************************************************
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanySsiDetailsRepository : ICompanySsiDetailsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanySsiDetailsRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseParticipationTransferData> GetUseCaseParticipationForCompany(Guid companyId, string language, DateTimeOffset minExpiry) =>
        _context.VerifiedCredentialTypes
            .Where(t => t.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.USE_CASE)
            .Select(t => new
            {
                UseCase = t.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new UseCaseParticipationTransferData(
                x.UseCase!.Name,
                x.UseCase.UseCaseDescriptions
                    .Where(ucd => ucd.LanguageShortName == language)
                    .Select(ucd => ucd.Description)
                    .SingleOrDefault(),
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new CompanySsiExternalTypeDetailTransferData(
                            new ExternalTypeDetailData(
                                e.Id,
                                e.VerifiedCredentialExternalTypeId,
                                e.Version,
                                e.Template,
                                e.ValidFrom,
                                e.Expiry),
                            e.CompanySsiDetails
                                .Where(ssi =>
                                    ssi.CompanyId == companyId &&
                                    ssi.VerifiedCredentialTypeId == x.TypeId &&
                                    ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                                    ssi.VerifiedCredentialExternalTypeDetailVersionId == e.Id &&
                                    ssi.ExpiryDate > minExpiry)
                                .Select(ssi =>
                                    new CompanySsiDetailTransferData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Document == null
                                            ? null
                                            : new DocumentData(
                                                ssi.Document!.Id,
                                                ssi.Document.DocumentName)))
                                .Take(2)
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(Guid companyId, DateTimeOffset minExpiry) =>
        _context.VerifiedCredentialTypes
            .Where(types => types.VerifiedCredentialTypeAssignedKind != null && types.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE)
            .Select(t => new
            {
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new SsiCertificateTransferData(
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new SsiCertificateExternalTypeDetailTransferData(
                            new ExternalTypeDetailData(
                                e.Id,
                                e.VerifiedCredentialExternalTypeId,
                                e.Version,
                                e.Template,
                                e.ValidFrom,
                                e.Expiry),
                            e.CompanySsiDetails
                                .Where(ssi =>
                                    ssi.CompanyId == companyId &&
                                    ssi.VerifiedCredentialTypeId == x.TypeId &&
                                    ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                                    ssi.ExpiryDate > minExpiry)
                                .Select(ssi =>
                                    new CompanySsiDetailTransferData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Document == null
                                            ? null
                                            : new DocumentData(
                                                ssi.Document!.Id,
                                                ssi.Document.DocumentName)))
                                .Take(2)
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public CompanySsiDetail CreateSsiDetails(Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, Guid docId, CompanySsiDetailStatusId companySsiDetailStatusId, Guid userId, Action<CompanySsiDetail>? setOptionalFields)
    {
        var detail = new CompanySsiDetail(Guid.NewGuid(), companyId, verifiedCredentialTypeId, companySsiDetailStatusId, docId, userId, DateTimeOffset.UtcNow);
        setOptionalFields?.Invoke(detail);
        return _context.CompanySsiDetails.Add(detail).Entity;
    }

    /// <inheritdoc />
    public Task<bool> CheckSsiDetailsExistsForCompany(Guid companyId, VerifiedCredentialTypeId verifiedCredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? verifiedCredentialExternalTypeUseCaseDetailId) =>
        _context.CompanySsiDetails
            .AnyAsync(x =>
                x.CompanyId == companyId &&
                x.VerifiedCredentialTypeId == verifiedCredentialTypeId &&
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == kindId &&
                x.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                (verifiedCredentialExternalTypeUseCaseDetailId == null || x.VerifiedCredentialExternalTypeDetailVersionId == verifiedCredentialExternalTypeUseCaseDetailId));

    /// <inheritdoc />
    public Task<DateTimeOffset> CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(Guid verifiedCredentialExternalTypeUseCaseDetailId, VerifiedCredentialTypeId verifiedCredentialTypeId) =>
        _context.VerifiedCredentialExternalTypeDetailVersions
            .Where(x =>
                x.Id == verifiedCredentialExternalTypeUseCaseDetailId &&
                x.VerifiedCredentialExternalType!.VerifiedCredentialTypeAssignedExternalTypes.Any(y => y.VerifiedCredentialTypeId == verifiedCredentialTypeId))
            .Select(x => x.Expiry)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, IEnumerable<Guid> DetailVersionIds)> CheckSsiCertificateType(VerifiedCredentialTypeId credentialTypeId) =>
        _context.VerifiedCredentialTypeAssignedKinds
            .Where(x =>
                x.VerifiedCredentialTypeId == credentialTypeId &&
                x.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE)
            .Select(x => new ValueTuple<bool, IEnumerable<Guid>>(
                true,
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions.Select(x => x.Id)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IQueryable<CompanySsiDetail> GetAllCredentialDetails(CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, string? companyName) =>
        _context.CompanySsiDetails.AsNoTracking()
            .Where(c =>
                (!companySsiDetailStatusId.HasValue || c.CompanySsiDetailStatusId == companySsiDetailStatusId.Value) &&
                (!credentialTypeId.HasValue || c.VerifiedCredentialTypeId == credentialTypeId) &&
                (companyName == null || EF.Functions.ILike(c.Company!.Name, $"%{companyName.EscapeForILike()}%")));

    /// <inheritdoc />
    public Task<(bool exists, SsiApprovalData data)> GetSsiApprovalData(Guid credentialId) =>
        _context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, SsiApprovalData>(
                true,
                new SsiApprovalData(
                    x.CompanySsiDetailStatusId,
                    x.VerifiedCredentialTypeId,
                    x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind == null ? null : x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId,
                    x.Company!.Name,
                    x.Company.BusinessPartnerNumber,
                    new DetailData(
                        x.VerifiedCredentialExternalTypeDetailVersion!.VerifiedCredentialExternalTypeId,
                        x.VerifiedCredentialExternalTypeDetailVersion.Template,
                        x.VerifiedCredentialExternalTypeDetailVersion.Version,
                        x.VerifiedCredentialExternalTypeDetailVersion.Expiry
                    ),
                    new SsiRequesterData(
                        x.CreatorUserId,
                        x.CreatorUser!.Email,
                        x.CreatorUser.Firstname,
                        x.CreatorUser.Lastname
                    )
                )
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, CompanySsiDetailStatusId Status, VerifiedCredentialTypeId Type, Guid RequesterId, string? RequesterEmail, string? Firstname, string? Lastname)> GetSsiRejectionData(Guid credentialId) =>
        _context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid, string?, string?, string?>(
                true,
                x.CompanySsiDetailStatusId,
                x.VerifiedCredentialTypeId,
                x.CreatorUserId,
                x.CreatorUser!.Email,
                x.CreatorUser.Firstname,
                x.CreatorUser.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyCompanySsiDetails(Guid id, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields)
    {
        var entity = new CompanySsiDetail(id, Guid.Empty, default, default, Guid.Empty, Guid.Empty, default);
        initialize?.Invoke(entity);
        _context.Attach(entity);
        updateFields.Invoke(entity);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes(Guid companyId) =>
        _context.VerifiedCredentialTypes
            .Where(x =>
                x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.CERTIFICATE &&
                !x.CompanySsiDetails.Any(ssi =>
                    ssi.CompanyId == companyId &&
                    (ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING || ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    public IAsyncEnumerable<CredentialExpiryData> GetExpiryData(DateTimeOffset now, DateTimeOffset inactiveVcsToDelete, DateTimeOffset expiredVcsToDelete) =>
        _context.CompanySsiDetails
            .Where(x =>
                (x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE && x.DateCreated < inactiveVcsToDelete) ||
                ((x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE || x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE) && x.ExpiryDate < expiredVcsToDelete) ||
                (x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING && x.ExpiryDate < now) ||
                (x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE &&
                    (x.ExpiryDate <= now.AddDays(1) && x.ExpiryCheckTypeId != ExpiryCheckTypeId.ONE_DAY) ||
                    (x.ExpiryDate <= now.AddDays(14) && x.ExpiryCheckTypeId != ExpiryCheckTypeId.TWO_WEEKS) ||
                    (x.ExpiryDate <= now.AddMonths(2) && x.ExpiryCheckTypeId == null)
                ))
            .Select(x => new CredentialExpiryData(
                x.Id,
                x.DateCreated,
                x.ExpiryDate!.Value,
                x.ExpiryCheckTypeId,
                x.VerifiedCredentialExternalTypeDetailVersion!.Version,
                x.CompanySsiDetailStatusId,
                x.VerifiedCredentialTypeId,
                new UserMailingData(
                    x.CreatorUserId,
                    x.CreatorUser!.Email,
                    x.CreatorUser.Firstname,
                    x.CreatorUser.Lastname)
            ))
            .ToAsyncEnumerable();

    public void RemoveSsiDetail(Guid companySsiDetailId) =>
        _context.CompanySsiDetails.Remove(new CompanySsiDetail(companySsiDetailId, Guid.Empty, default, default, Guid.Empty, Guid.Empty, default));
}

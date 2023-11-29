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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IDocumentRepository"/> accessing database with EF Core.
public class DocumentRepository : IDocumentRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbContext">PortalDb context.</param>
    public DocumentRepository(PortalDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    /// <inheritdoc />
    public Document CreateDocument(string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields)
    {
        var document = new Document(
            Guid.NewGuid(),
            documentContent,
            hash,
            documentName,
            mediaTypeId,
            DateTimeOffset.UtcNow,
            DocumentStatusId.PENDING,
            documentTypeId);

        setupOptionalFields?.Invoke(document);
        return _dbContext.Documents.Add(document).Entity;
    }

    /// <inheritdoc />
    public Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> ConsentIds, bool IsSameUser)> GetDocumentDetailsForIdUntrackedAsync(Guid documentId, Guid companyUserId) =>
        _dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(document => ((Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> ConsentIds, bool IsSameUser))
                new(document.Id,
                    document.DocumentStatusId,
                    document.Consents.Select(consent => consent.Id),
                    document.CompanyUserId == companyUserId))
            .SingleOrDefaultAsync();

    public Task<(bool IsApplicationAssignedUser, IEnumerable<UploadDocuments> Documents)> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, Guid companyUserId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => new
            {
                IsApplicationAssignedUser = application.Invitations.Any(invitation => invitation.CompanyUserId == companyUserId),
                Invitations = application.Invitations
            })
            .Select(x => new ValueTuple<bool, IEnumerable<UploadDocuments>>(
                x.IsApplicationAssignedUser,
                x.Invitations.SelectMany(invitation =>
                    invitation.CompanyUser!.Documents
                        .Where(document => x.IsApplicationAssignedUser && document.DocumentTypeId == documentTypeId)
                        .Select(document =>
                            new UploadDocuments(
                                document!.Id,
                                document!.DocumentName))))
            )
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid DocumentId, bool IsSameUser)> GetDocumentIdWithCompanyUserCheckAsync(Guid documentId, Guid companyUserId) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new ValueTuple<Guid, bool>(x.Id, x.CompanyUserId == companyUserId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(byte[]? Content, string FileName, MediaTypeId MediaTypeId, bool IsUserInCompany)> GetDocumentDataAndIsCompanyUserAsync(Guid documentId, Guid userCompanyId) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new
            {
                Document = x,
                IsUserInSameCompany = x.CompanyUser!.Identity!.CompanyId == userCompanyId
            })
            .Select(x => new ValueTuple<byte[]?, string, MediaTypeId, bool>(
                x.IsUserInSameCompany ? x.Document.DocumentContent : null,
                x.Document.DocumentName,
                x.Document.MediaTypeId,
                x.IsUserInSameCompany))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(byte[] Content, string FileName, MediaTypeId MediaTypeId)> GetDocumentDataByIdAndTypeAsync(Guid documentId, DocumentTypeId documentTypeId) =>
        _dbContext.Documents
        .Where(x => x.Id == documentId && x.DocumentTypeId == documentTypeId)
        .Select(x => new ValueTuple<byte[], string, MediaTypeId>(x.DocumentContent, x.DocumentName, x.MediaTypeId))
        .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void RemoveDocument(Guid documentId) =>
        _dbContext.Documents.Remove(new Document(documentId, null!, null!, null!, default, default, default, default));

    /// <inheritdoc />
    public Task<Document?> GetDocumentByIdAsync(Guid documentId) =>
        this._dbContext.Documents.SingleOrDefaultAsync(x => x.Id == documentId);

    /// <inheritdoc />
    public Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, bool IsSameApplicationUser, DocumentTypeId documentTypeId, bool IsQueriedApplicationStatus, IEnumerable<Guid> applicationId)> GetDocumentDetailsForApplicationUntrackedAsync(Guid documentId, Guid userCompanyId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds) =>
        _dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(document => new
            {
                Document = document,
                Applications = document.CompanyUser!.Identity!.Company!.CompanyApplications
            })
            .Select(x => new ValueTuple<Guid, DocumentStatusId, bool, DocumentTypeId, bool, IEnumerable<Guid>>(
                x.Document.Id,
                x.Document.DocumentStatusId,
                x.Applications.Any(companyApplication => companyApplication.CompanyId == userCompanyId),
                x.Document.DocumentTypeId,
                x.Applications.Any(companyApplication => applicationStatusIds.Contains(companyApplication.ApplicationStatusId)),
                x.Applications.Select(x => x.Id)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyDocument(Guid documentId, Action<Document>? initialize, Action<Document> modify)
    {
        var document = new Document(documentId, null!, null!, null!, default, default, default, default);
        initialize?.Invoke(document);
        _dbContext.Attach(document);
        modify(document);
    }

    public void AttachAndModifyDocuments(IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> documentData)
    {
        var initial = documentData.Select(x =>
            {
                var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                x.Initialize?.Invoke(document);
                return (Document: document, x.Modify);
            }
        ).ToList();
        _dbContext.AttachRange(initial.Select(x => x.Document));
        initial.ForEach(x => x.Modify(x.Document));
    }

    /// <inheritdoc />
    public Task<DocumentSeedData?> GetDocumentSeedDataByIdAsync(Guid documentId) =>
        _dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(doc => new DocumentSeedData(
                doc.Id,
                doc.DateCreated,
                doc.DocumentName,
                (int)doc.DocumentTypeId,
                doc.CompanyUserId,
                doc.DocumentHash,
                doc.DocumentContent,
                (int)doc.DocumentStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferDocumentContentData?> GetOfferDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIds, OfferTypeId offerTypeId, CancellationToken cancellationToken) =>
        _dbContext.Documents
            .Where(document => document.Id == documentId)
            .Select(document => new
            {
                Offer = document.Offers.SingleOrDefault(offer => offer.Id == offerId),
                Document = document
            })
            .Select(x => new
            {
                IsValidDocumentType = documentTypeIds.Contains(x.Document.DocumentTypeId),
                IsDocumentLinkedToOffer = x.Offer != null,
                IsValidOfferType = x.Offer!.OfferTypeId == offerTypeId,
                IsInactive = x.Document.DocumentStatusId == DocumentStatusId.INACTIVE,
                Document = x.Document
            })
            .Select(x => new OfferDocumentContentData(
                x.IsValidDocumentType,
                x.IsDocumentLinkedToOffer,
                x.IsValidOfferType,
                x.IsInactive,
                x.IsValidDocumentType && x.IsDocumentLinkedToOffer && x.IsValidOfferType && !x.IsInactive ? x.Document.DocumentContent : null,
                x.Document.DocumentName,
                x.Document.MediaTypeId
            ))
            .SingleOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public Task<(IEnumerable<(OfferStatusId OfferStatusId, Guid OfferId, bool IsOfferType)> OfferData, bool IsDocumentTypeMatch, DocumentStatusId DocumentStatusId, bool IsProviderCompanyUser)> GetOfferDocumentsAsync(Guid documentId, Guid userCompanyId, IEnumerable<DocumentTypeId> documentTypeIds, OfferTypeId offerTypeId) =>
        _dbContext.Documents
            .Where(document => document.Id == documentId)
            .Select(document => new
            {
                Offers = document.Offers,
                Document = document
            })
            .Select(x => new
            {
                IsOfferAssignedDocument = x.Offers.Any(),
                OfferData = x.Offers.Select(o => new ValueTuple<OfferStatusId, Guid, bool>(o.OfferStatusId, o.Id, o.OfferTypeId == offerTypeId)),
                IsDocumentTypeMatch = documentTypeIds.Contains(x.Document.DocumentTypeId),
                DocumentStatus = x.Document.DocumentStatusId,
                IsProviderCompanyUser = x.Document.CompanyUser!.Identity!.CompanyId == userCompanyId
            })
            .Select(x => new ValueTuple<IEnumerable<ValueTuple<OfferStatusId, Guid, bool>>, bool, DocumentStatusId, bool>(
                x.OfferData,
                x.IsDocumentTypeMatch,
                x.DocumentStatus,
                x.IsProviderCompanyUser
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void RemoveDocuments(IEnumerable<Guid> documentIds) =>
        _dbContext.Documents.RemoveRange(documentIds.Select(documentId => new Document(documentId, null!, null!, null!, default, default, default, default)));

    public Task<(byte[] Content, string FileName, bool IsDocumentTypeMatch, MediaTypeId MediaTypeId)> GetDocumentAsync(Guid documentId, IEnumerable<DocumentTypeId> documentTypeIds) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new ValueTuple<byte[], string, bool, MediaTypeId>(x.DocumentContent, x.DocumentName, documentTypeIds.Contains(x.DocumentTypeId), x.MediaTypeId))
            .SingleOrDefaultAsync();

}

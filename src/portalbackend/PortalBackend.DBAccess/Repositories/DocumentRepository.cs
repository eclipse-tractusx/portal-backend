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
    public Document CreateDocument(string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentType, Action<Document>? setupOptionalFields)
    {
        var document = new Document(
            Guid.NewGuid(),
            documentContent,
            hash,
            documentName,
            DateTimeOffset.UtcNow,
            DocumentStatusId.PENDING,
            documentType);

        setupOptionalFields?.Invoke(document);
        return _dbContext.Documents.Add(document).Entity;
    }

    /// <inheritdoc />
    public Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> ConsentIds, bool IsSameUser)> GetDocumentDetailsForIdUntrackedAsync(Guid documentId, string iamUserId) =>
        _dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(document => ((Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> ConsentIds, bool IsSameUser))
                new (document.Id,
                    document.DocumentStatusId,
                    document.Consents.Select(consent => consent.Id),
                    document.CompanyUser!.IamUser!.UserEntityId == iamUserId))
            .SingleOrDefaultAsync();

    public Task<(bool IsApplicationAssignedUser, IEnumerable<UploadDocuments> Documents)> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(application => application.Id == applicationId)
            .Select(application => new {
                IsApplicationAssignedUser = application.Invitations.Any(invitation => invitation.CompanyUser!.IamUser!.UserEntityId == iamUserId),
                Invitations = application.Invitations
            })
            .Select(x => new ValueTuple<bool,IEnumerable<UploadDocuments>>(
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
    public Task<(Guid DocumentId, bool IsSameUser)> GetDocumentIdCompanyUserSameAsIamUserAsync(Guid documentId, string iamUserId) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new ValueTuple<Guid, bool>(x.Id, x.CompanyUser!.IamUser!.UserEntityId == iamUserId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(byte[]? Content, string FileName, bool IsUserInCompany)> GetDocumentDataAndIsCompanyUserAsync(Guid documentId, string iamUserId) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => new {
                Document = x,
                IsUserInSameCompany = x.CompanyUser!.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)
            })
            .Select(x => new ValueTuple<byte[]?, string, bool>(
                x.IsUserInSameCompany ? x.Document.DocumentContent : null,
                x.Document.DocumentName,
                x.IsUserInSameCompany))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(byte[] Content, string FileName)> GetDocumentDataByIdAndTypeAsync(Guid documentId, DocumentTypeId documentTypeId) =>
        _dbContext.Documents
        .Where(x => x.Id == documentId && x.DocumentTypeId == documentTypeId)
        .Select(x => new ValueTuple<byte[], string>(x.DocumentContent, x.DocumentName))
        .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void RemoveDocument(Guid documentId) => 
        _dbContext.Documents.Remove(new Document(documentId, null!, null!, null!, default, default, default));

    /// <inheritdoc />
    public Task<Document?> GetDocumentByIdAsync(Guid documentId) =>
        this._dbContext.Documents.SingleOrDefaultAsync(x => x.Id == documentId);

    /// <inheritdoc />
    public Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, bool IsSameApplicationUser, DocumentTypeId documentTypeId, bool IsQueriedApplicationStatus)> GetDocumentDetailsForApplicationUntrackedAsync(Guid documentId, string iamUserId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds) =>
        _dbContext.Documents
            .AsNoTracking()
            .Where(x => x.Id == documentId)
            .Select(document => new {
                Document = document,
                Applications = document.CompanyUser!.Company!.CompanyApplications
            })
            .Select(x => new ValueTuple<Guid, DocumentStatusId, bool, DocumentTypeId, bool>(
                x.Document.Id,
                x.Document.DocumentStatusId,
                x.Applications.Any(companyApplication => companyApplication.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)),
                x.Document.DocumentTypeId,
                x.Applications.Any(companyApplication => applicationStatusIds.Contains(companyApplication.ApplicationStatusId))))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyDocument(Guid documentId, Action<Document>? initialize, Action<Document> modify)
    {
        var document = new Document(documentId, null!, null!, null!, default, default, default);
        initialize?.Invoke(document);
        _dbContext.Attach(document);
        modify(document);
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
    public Task<(bool IsValidDocumentType, bool IsDocumentLinkedToOffer, bool IsValidOfferType, byte[]? Content, bool IsDocumentExisting, string FileName)> GetOfferImageDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIds, OfferTypeId offerTypeId, CancellationToken cancellationToken) =>
        _dbContext.Documents
            .Where(document => document.Id == documentId)
            .Select(document => new {
                Offer = document.Offers.SingleOrDefault(offer => offer.Id == offerId),
                Document = document
            })
            .Select(x => new {
                IsValidDocumentType = documentTypeIds.Contains(x.Document.DocumentTypeId),
                IsDocumentLinkedToOffer = x.Offer != null,
                IsValidOfferType = x.Offer!.OfferTypeId == offerTypeId,
                Document = x.Document
            })
            .Select(x => new ValueTuple<bool, bool, bool, byte[]?, bool, string>(
                x.IsValidDocumentType,
                x.IsDocumentLinkedToOffer,
                x.IsValidOfferType,
                x.IsValidDocumentType && x.IsDocumentLinkedToOffer && x.IsValidOfferType ? x.Document.DocumentContent : null,
                true,
                x.Document.DocumentName
            ))
            .SingleOrDefaultAsync(cancellationToken);
}

/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    public IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .SelectMany(iamUser => iamUser.CompanyUser!.Documents.Where(docu => docu.DocumentTypeId == documentTypeId && docu.DocumentStatusId != DocumentStatusId.INACTIVE))
            .Select(document =>
                new UploadDocuments(
                    document!.Id,
                    document!.DocumentName))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(Guid DocumentId, bool IsSameUser)> GetDocumentIdCompanyUserSameAsIamUserAsync(Guid documentId, string iamUserId) =>
        this._dbContext.Documents
            .Where(x => x.Id == documentId)
            .Select(x => ((Guid DocumentId, bool IsSameUser))new (x.Id, x.CompanyUser!.IamUser!.UserEntityId == iamUserId))
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
}

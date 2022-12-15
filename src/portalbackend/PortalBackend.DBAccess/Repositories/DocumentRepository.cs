/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
    
    /// <inheritdoc />
    public Task<DocumentStatusIdData?> GetDocumentStatusIdAsync(Guid CompanyUserId,string iamUserId)=>
        _dbContext.Documents
            .Where(x => x.CompanyUserId == CompanyUserId)
            .Select(document => new {
                Document = document,
                Applications = document.CompanyUser!.Company!.CompanyApplications
            })
            .Select(X=> new DocumentStatusIdData(
                X.Document.Id,
                X.Document.CompanyUserId,
                X.Document.DocumentStatusId,
                X.Applications.Any(companyApplication => companyApplication.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            )).SingleOrDefaultAsync();
    public void AttachAndModifyDocument(Guid documentId, Action<Document> setOptionalParameters)
    {
        var document = _dbContext.Attach(new Document(documentId,default!, default!, default!,default,default,default)).Entity;
        setOptionalParameters.Invoke(document);
    }
}

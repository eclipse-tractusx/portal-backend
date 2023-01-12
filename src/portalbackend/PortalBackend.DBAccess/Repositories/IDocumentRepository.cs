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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Creates a document in the persistence layer.
    /// </summary>
    /// <param name="documentName">The documents name</param>
    /// <param name="documentContent">The document itself</param>
    /// <param name="hash">Hash of the document</param>
    /// <param name="documentType">the document type id</param>
    /// <param name="setupOptionalFields">Action to setup the additional fields</param>
    /// <returns>Returns the created document</returns>
    Document CreateDocument(string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentType, Action<Document>? setupOptionalFields);

    /// <summary>
    /// Gets the document with the given id from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>Returns the document</returns>
    Task<Document?> GetDocumentByIdAsync(Guid documentId);
    
    Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, IEnumerable<Guid> ConsentIds, bool IsSameUser)> GetDocumentDetailsForIdUntrackedAsync(Guid documentId, string iamUserId);

    /// <summary>
    /// Gets all documents for the given applicationId, documentId and userId
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="documentTypeId">Id of the document type</param>
    /// <param name="iamUserId">Id of the user</param>
    /// <returns>A collection of documents</returns>
    Task<(bool IsApplicationAssignedUser, IEnumerable<UploadDocuments> Documents)> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId);
    
    /// <summary>
    /// Gets the documents userid by the document id
    /// </summary>
    /// <param name="documentId">id of the document the user id should be selected for</param>
    /// <returns>Returns the user id if a document is found for the given id, otherwise null</returns>
    Task<(Guid DocumentId, bool IsSameUser)> GetDocumentIdCompanyUserSameAsIamUserAsync(Guid documentId, string iamUserId);

    /// <summary>
    ///Deleting document record and document file from the portal db/document storage location
    /// </summary>
    /// <param name="documentId">The documentId that should be removed</param>
    void RemoveDocument(Guid documentId);
    
    /// <summary>
    /// Gets the documents and User by the document id
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="iamUserId"></param>
    /// <param name="applicationStatusIds"></param>
    Task<(Guid DocumentId, DocumentStatusId DocumentStatusId, bool IsSameApplicationUser, DocumentTypeId documentTypeId, bool IsQueriedApplicationStatus)> GetDocumentDetailsForApplicationUntrackedAsync(Guid documentId, string iamUserId, IEnumerable<CompanyApplicationStatusId> applicationStatusIds);

    /// <summary>
    /// Attaches the document and sets the optional parameters
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <param name="initialize">Action to initialize the entity with values before the change</param>
    /// <param name="modify">Action to set the values that are subject to change</param>
    void AttachAndModifyDocument(Guid documentId, Action<Document>? initialize, Action<Document> modify);

    /// <summary>
    /// Gets the document seed data for the given id
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>The document seed data</returns>
    Task<DocumentSeedData?> GetDocumentSeedDataByIdAsync(Guid documentId);

    /// <summary>
    /// Retrieve Document TypeId , Content and validate app link to document
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentId"></param>
    /// <param name="appDocumentTypeIds"></param>
    /// <returns></returns>
    Task<(bool IsValidDocumentType, bool IsDocumentLinkedToOffer, bool IsValidOfferType, byte[]? Content, bool IsDocumentExisting)> GetOfferImageDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIds, OfferTypeId offerTypeId);
}

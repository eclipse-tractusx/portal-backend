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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for document handling
/// </summary>
public class DocumentsBusinessLogic : IDocumentsBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly DocumentSettings _settings;

    /// <summary>
    /// Creates a new instance <see cref="DocumentsBusinessLogic"/>
    /// </summary>
    public DocumentsBusinessLogic(IPortalRepositories portalRepositories, IOptions<DocumentSettings> options)
    {
        _portalRepositories = portalRepositories;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId)
    {
        var document = await this._portalRepositories.GetInstance<IDocumentRepository>().GetDocumentByIdAsync(documentId).ConfigureAwait(false);
        if (document is null)
        {
            throw new NotFoundException("No document with the given id was found.");
        }

        return (document.DocumentName, document.DocumentContent);
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteDocumentAsync(Guid documentId, string iamUserId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForIdUntrackedAsync(documentId, iamUserId).ConfigureAwait(false);

        if (details.DocumentId == Guid.Empty)
        {
            throw new NotFoundException("Document is not existing");
        }

        if (!details.IsSameUser)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }

        if (details.DocumentStatusId == DocumentStatusId.LOCKED)
        {
            throw new ArgumentException("Incorrect document status");
        }

        documentRepository.RemoveDocument(details.DocumentId);
        if (details.ConsentIds.Any())
        {
            _portalRepositories.GetInstance<IConsentRepository>().RemoveConsents(details.ConsentIds.Select(x => new Consent(x)));
        }

        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<DocumentSeedData> GetSeedData(Guid documentId)
    {
        if (!_settings.EnableSeedEndpoint)
        {
            throw new ForbiddenException("Endpoint can only be used on dev environment");
        }

        var document = await _portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentSeedDataByIdAsync(documentId)
            .ConfigureAwait(false);
        if (document == null)
        {
            throw new NotFoundException($"Document {documentId} does not exists.");
        }

        return document;
    }
}

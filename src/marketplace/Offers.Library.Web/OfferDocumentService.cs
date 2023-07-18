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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;

public class OfferDocumentService : IOfferDocumentService
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    public OfferDocumentService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    public async Task UploadDocumentAsync(Guid id, DocumentTypeId documentTypeId, IFormFile document, (Guid UserId, Guid CompanyId) identity, OfferTypeId offerTypeId, IEnumerable<UploadDocumentConfig> uploadDocumentTypeIdSettings, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            throw new ControllerArgumentException($"{offerTypeId} id should not be null");
        }

        if (string.IsNullOrEmpty(document.FileName))
        {
            throw new ControllerArgumentException("File name should not be null");
        }

        var uploadContentTypeSettings = uploadDocumentTypeIdSettings.FirstOrDefault(x => x.DocumentTypeId == documentTypeId);
        if (uploadContentTypeSettings == null)
        {
            throw new ControllerArgumentException($"documentType must be either: {string.Join(",", uploadDocumentTypeIdSettings.Select(x => x.DocumentTypeId))}");
        }
        // Check if document is a pdf,jpeg and png file (also see https://www.rfc-editor.org/rfc/rfc3778.txt)
        MediaTypeId mediaTypeId;
        try
        {
            mediaTypeId = document.ContentType.ParseMediaTypeId();
        }
        catch (UnsupportedMediaTypeException e)
        {
            throw new UnsupportedMediaTypeException($"Document type {documentTypeId}, {e.Message}. File with contentType :{string.Join(",", uploadContentTypeSettings.MediaTypes)} are allowed.");
        }
        if (!uploadContentTypeSettings.MediaTypes.Contains(mediaTypeId))
        {
            throw new UnsupportedMediaTypeException($"Document type {documentTypeId}, mediaType '{document.ContentType}' is not supported. File with contentType :{string.Join(",", uploadContentTypeSettings.MediaTypes)} are allowed.");
        }

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var result = await offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, identity.CompanyId, OfferStatusId.CREATED, offerTypeId).ConfigureAwait(false);

        if (result == default)
        {
            throw new NotFoundException($"{offerTypeId} {id} does not exist");
        }

        if (!result.IsStatusMatching)
            throw new ConflictException("offerStatus is in Incorrect State");

        if (!result.IsUserOfProvider)
        {
            throw new ForbiddenException($"Company {identity.CompanyId} is not the provider company of {offerTypeId} {id}");
        }

        var (content, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(false);

        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, content, hash, mediaTypeId, documentTypeId, x =>
        {
            x.CompanyUserId = identity.UserId;
        });
        _portalRepositories.GetInstance<IOfferRepository>().CreateOfferAssignedDocument(id, doc.Id);

        _portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(id, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}

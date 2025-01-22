/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
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
    private readonly IIdentityData _identityData;
    private readonly DocumentSettings _settings;

    /// <summary>
    /// Creates a new instance <see cref="DocumentsBusinessLogic"/>
    /// </summary>
    public DocumentsBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IOptions<DocumentSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetDocumentAsync(Guid documentId)
    {
        var documentDetails = await _portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentDataAndIsCompanyUserAsync(documentId, _identityData.CompanyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (documentDetails == default)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        if (!documentDetails.IsUserInCompany)
        {
            throw ForbiddenException.Create(AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_ACCESS_DOC);
        }

        if (documentDetails.Content == null)
        {
            throw UnexpectedConditionException.Create(AdministrationDocumentErrors.DOCUMENT_UNEXPECT_DOC_CONTENT_NOT_NULL);
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetSelfDescriptionDocumentAsync(Guid documentId)
    {
        var documentDetails = await _portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentDataByIdAndTypeAsync(documentId, DocumentTypeId.SELF_DESCRIPTION)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (documentDetails == default)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_SELFDESP_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }
        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    /// <inheritdoc />
    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForIdUntrackedAsync(documentId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (details.DocumentId == Guid.Empty)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        if (!details.IsSameUser)
        {
            throw ForbiddenException.Create(AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC);
        }

        if (details.DocumentStatusId == DocumentStatusId.LOCKED)
        {
            throw ControllerArgumentException.Create(AdministrationDocumentErrors.DOCUMENT_ARGUMENT_INCORR_DOC_STATUS);
        }

        documentRepository.RemoveDocument(details.DocumentId);
        if (details.ConsentIds.Any())
        {
            _portalRepositories.GetInstance<IConsentRepository>().RemoveConsents(details.ConsentIds.Select(x => new Consent(x, Guid.Empty, Guid.Empty, Guid.Empty, default, default)));
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return true;
    }

    /// <inheritdoc />
    public async Task<DocumentSeedData> GetSeedData(Guid documentId)
    {
        if (!_settings.EnableSeedEndpoint)
        {
            throw ForbiddenException.Create(AdministrationDocumentErrors.DOCUMENT_FORBIDDEN_ENDPOINT_ALLOW_USE_IN_DEV_ENV);
        }

        var document = await _portalRepositories.GetInstance<IDocumentRepository>()
            .GetDocumentSeedDataByIdAsync(documentId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (document == null)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        return document;
    }

    /// <inheritdoc />
    public async Task<(string fileName, byte[] content)> GetFrameDocumentAsync(Guid documentId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();

        var documentDetails = await documentRepository.GetDocumentAsync(documentId, _settings.FrameDocumentTypeIds).ConfigureAwait(ConfigureAwaitOptions.None);
        if (documentDetails == default)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }
        if (!documentDetails.IsDocumentTypeMatch)
        {
            throw NotFoundException.Create(AdministrationDocumentErrors.DOCUMENT_NOT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        return (documentDetails.FileName, documentDetails.Content);
    }
}

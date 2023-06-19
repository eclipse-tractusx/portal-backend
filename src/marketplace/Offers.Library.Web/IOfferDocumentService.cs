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
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;

/// <summary>
/// Business logic for handling offer-related operations. Includes persistence layer access.
/// </summary>
public interface IOfferDocumentService
{
    /// <summary>
    /// Upload Document the given offertypeId by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="identity"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="uploadDocumentTypeIdSettings"></param>
    /// <param name="cancellationToken"></param>
    Task UploadDocumentAsync(Guid id, DocumentTypeId documentTypeId, IFormFile document, (Guid UserId, Guid CompanyId) identity, OfferTypeId offerTypeId, IEnumerable<UploadDocumentConfig> uploadDocumentTypeIdSettings, CancellationToken cancellationToken);
}

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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public interface IDocumentsBusinessLogic
{
    /// <summary>
    /// Gets the document with the given id
    /// </summary>
    /// <param name="documentId">Id of the document to get</param>
    /// <returns>Returns the filename and content of the file</returns>
    Task<(string FileName, byte[] Content, string MediaType)> GetDocumentAsync(Guid documentId);

    /// <summary>
    /// Gets the selfdescription document with the given id
    /// </summary>
    /// <param name="documentId">Id of the document to get</param>
    /// <returns>Returns the filename and content of the file</returns>
    Task<(string FileName, byte[] Content, string MediaType)> GetSelfDescriptionDocumentAsync(Guid documentId);

    /// <summary>
    /// Deletes the document and the corresponding consent from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document that should be deleted</param>
    /// <returns>Returns <c>true</c> if the document and corresponding consent were deleted successfully. Otherwise a specific error is thrown.</returns>
    Task<bool> DeleteDocumentAsync(Guid documentId);

    /// <summary>
    /// Gets the document as json for the seeding data
    /// </summary>
    /// <param name="documentId">Id of the document</param>
    /// <returns>The document as json</returns>
    Task<DocumentSeedData> GetSeedData(Guid documentId);

    /// <summary>
    /// Retrieve Frame Document
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    Task<(string fileName, byte[] content)> GetFrameDocumentAsync(Guid documentId);
}

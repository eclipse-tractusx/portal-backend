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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// returs the App Delete Document states
/// </summary>

public record AppDeleteDocumentStats(int Success, int Error, IEnumerable<DeleteDocumentErrorDetails> Errors);

/// <summary>
/// Document Delete error details
/// </summary>
public record DeleteDocumentErrorDetails(Guid DocumentId, string Reasons);

/// <summary>
/// returs the App Upload Document states
/// </summary>
public record AppUploadDocumentStats(int Success, int Error, IEnumerable<UploadDocumentErrorDetails> Errors);

/// <summary>
/// Document Delete error details
/// </summary>
public record UploadDocumentErrorDetails(string DocumentName, string Reasons);

/// <summary>
/// Upload Multiple Document Data 
/// </summary>
public class UploadMulipleDocuments
{

    /// <summary>
    /// DocumentType id
    /// </summary>
    /// <value></value>
    public DocumentTypeId DocumentTypeId { get; set; }

    /// <summary>
    /// Upload Document data
    /// </summary>
    /// <value></value>
    public IFormFile Document { get; set; } = null!;
}


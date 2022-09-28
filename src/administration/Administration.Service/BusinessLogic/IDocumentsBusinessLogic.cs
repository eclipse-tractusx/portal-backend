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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Repository for writing documents on persistence layer.
/// </summary>
public interface IDocumentsBusinessLogic
{
    Task<(string fileName, byte[] content)> GetDocumentAsync(Guid documentId, string iamUserId);
    
    /// <summary>
    /// Deletes the document and the corresponding consent from the persistence layer.
    /// </summary>
    /// <param name="documentId">Id of the document that should be deleted</param>
    /// <param name="iamUserId"></param>
    /// <returns>Returns <c>true</c> if the document and corresponding consent were deleted successfully. Otherwise a specific error is thrown.</returns>
    Task<bool> DeleteDocumentAsync(Guid documentId, string iamUserId);
}

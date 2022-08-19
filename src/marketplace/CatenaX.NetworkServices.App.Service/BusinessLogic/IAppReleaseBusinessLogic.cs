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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// Business logic for handling app release-related operations. Includes persistence layer access.
/// </summary>
public interface IAppReleaseBusinessLogic
{
    /// <summary>
    /// Update an App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="updateModel"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task UpdateAppAsync(Guid appId, AppEditableDetail updateModel, string userId);
    
    /// <summary>
    /// Upload document for given company user for appId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task UpdateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string userId);
}

﻿/********************************************************************************
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

using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for handling connector api requests.
/// </summary>
public interface IConnectorsBusinessLogic
{
    /// <summary>
    /// Get all of a user's company's connectors by iam user ID.
    /// </summary>
    /// <param name="iamUserId">ID of the user to retrieve company connectors for.</param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>AsyncEnumerable of the result connectors.</returns>
    Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatasForIamUserAsyncEnum(string iamUserId, int page, int size);

    Task<ConnectorData> GetCompanyConnectorDataForIdIamUserAsync(Guid connectorId, string iamUserId);

    /// <summary>
    /// Add a connector to persistence layer and calls the sd factory service with connector parameters.
    /// </summary>
    /// <param name="connectorInputModel">Connector parameters for creation.</param>
    /// <returns>View model of created connector.</returns>
    /// <param name="accessToken">Bearer token to be used for authorizing the sd factory request.</param>
    Task<ConnectorData> CreateConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken);

    /// <summary>
    /// Remove a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    Task DeleteConnectorAsync(Guid connectorId);
}

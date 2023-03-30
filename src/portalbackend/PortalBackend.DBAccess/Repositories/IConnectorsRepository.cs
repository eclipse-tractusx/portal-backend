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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing connectors on persistence layer.
/// </summary>
public interface IConnectorsRepository
{
    /// <summary>
    /// Get all connectors of a user's company by iam user ID.
    /// </summary>
    /// <param name="iamUserId">ID of the iam user used to determine company's connectors for.</param>
    /// <returns>Queryable of connectors that allows transformation.</returns>
    IQueryable<Connector> GetAllCompanyConnectorsForIamUser(string iamUserId);

    Task<(ConnectorData ConnectorData, bool IsProviderUser)> GetConnectorByIdForIamUser(Guid connectorId, string iamUser);

    Task<(ConnectorInformationData ConnectorInformationData, bool IsProviderUser)> GetConnectorInformationByIdForIamUser(Guid connectorId, string iamUser);

    /// <summary>
    /// Creates a given connector in persistence layer. 
    /// </summary>
    /// <param name="name">Name of the connector to create.</param>
    /// <param name="location">Location of the connector.</param>
    /// <param name="connectorUrl">Url of the connector to create.</param>
    /// <param name="setupOptionalFields">Action to setup optional fields.</param>
    /// <returns>Created and persisted connector.</returns>
    Connector CreateConnector(string name, string location, string connectorUrl, Action<Connector>? setupOptionalFields);

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    void DeleteConnector(Guid connectorId);
    
    /// <summary>
    /// Get Connector End Point Grouped By Business Partner Number
    /// </summary>
    /// <param name="bpns"></param>
    /// <returns></returns>
    IAsyncEnumerable<(string BusinessPartnerNumber, string ConnectorEndpoint)> GetConnectorEndPointDataAsync(IEnumerable<string> bpns);
    
    /// <summary>
    /// Attaches the entity with the given id to the db context and modifies the given values.
    /// </summary>
    /// <param name="connectorId">Id of the connector</param>
    /// <param name="setOptionalParameters">Action to set the parameters</param>
    /// <returns>The updated connector</returns>
    Connector AttachAndModifyConnector(Guid connectorId, Action<Connector> setOptionalParameters);

    /// <summary>
    /// Checks whether the connector exists
    /// </summary>
    /// <param name="connectorId">Id of the connector</param>
    /// <returns><c>true</c> if the connector exists, otherwise <c>false</c></returns>
    Task<(Guid ConnectorId, Guid? SelfDescriptionDocumentId)> GetConnectorDataById(Guid connectorId);

    /// <summary>
    /// Gets SelfDescriptionDocument Data
    /// </summary>
    /// <param name="connectorId">Id of the connector</param>
    /// <returns>returns SelfDescriptionDocument Data/c></returns>
    Task<(bool IsConnectorIdExist, Guid? SelfDescriptionDocumentId, DocumentStatusId? DocumentStatusId)> GetSelfDescriptionDocumentDataAsync(Guid connectorId);
}

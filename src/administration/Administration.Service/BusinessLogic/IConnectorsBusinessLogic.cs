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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for handling connector api requests.
/// </summary>
public interface IConnectorsBusinessLogic
{
    /// <summary>
    /// Get all of a user's company's connectors by iam user ID.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>AsyncEnumerable of the result connectors.</returns>
    Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatas(int page, int size);

    /// <summary>
    /// Get all of a user's company's connectors by iam user ID.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>AsyncEnumerable of the result connectors.</returns>
    Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForCompany(int page, int size);

    Task<ConnectorData> GetCompanyConnectorData(Guid connectorId);

    /// <summary>
    /// Add a connector to persistence layer and calls the sd factory service with connector parameters.
    /// </summary>
    /// <param name="connectorInputModel">Connector parameters for creation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>View model of created connector.</returns>
    Task<Guid> CreateConnectorAsync(ConnectorInputModel connectorInputModel, CancellationToken cancellationToken);

    /// <summary>
    /// Add a managed connector to persistence layer and calls the sd factory service with connector parameters.
    /// </summary>
    /// <param name="connectorInputModel">Connector parameters for creation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>View model of created connector.</returns>
    Task<Guid> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken);

    /// <summary>
    /// Remove a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    Task DeleteConnectorAsync(Guid connectorId);

    /// <summary>
    /// Retrieve connector end point along with bpns
    /// </summary>
    /// <param name="bpns"></param>
    /// <returns></returns>
    IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync(IEnumerable<string>? bpns);

    /// <summary>
    /// Processes the clearinghouse self description
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, CancellationToken cancellationToken);

    /// <summary>
    /// Update the connector url
    /// </summary>
    /// <param name="connectorId">Id of the connector</param>
    /// <param name="data">Update data for the connector</param>
    Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data);

    /// <summary>
    /// Gets the offer subscription data
    /// </summary>
    /// <param name="connectorIdSet" example="false">
    /// Optional: if <c>true</c> only respond with subscriptions where a link to a connector is given,
    /// if <c>false</c> it will only return subscriptions where no link to an connector exists.
    /// </param>
    /// <returns>Returns an IAsyncEnumerable of <see cref="OfferSubscriptionConnectorData"/></returns>
    IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData(bool? connectorIdSet);
}

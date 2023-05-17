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
	/// <param name="iamUserId">ID of the user to retrieve company connectors for.</param>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns>AsyncEnumerable of the result connectors.</returns>
	Task<Pagination.Response<ConnectorData>> GetAllCompanyConnectorDatasForIamUserAsync(string iamUserId, int page, int size);

	/// <summary>
	/// Get all of a user's company's connectors by iam user ID.
	/// </summary>
	/// <param name="iamUserId">ID of the user to retrieve company connectors for.</param>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns>AsyncEnumerable of the result connectors.</returns>
	Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorForIamUserAsync(string iamUserId, int page, int size);

	Task<ConnectorData> GetCompanyConnectorDataForIdIamUserAsync(Guid connectorId, string iamUserId);

	/// <summary>
	/// Add a connector to persistence layer and calls the sd factory service with connector parameters.
	/// </summary>
	/// <param name="connectorInputModel">Connector parameters for creation.</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken"></param>
	/// <returns>View model of created connector.</returns>
	Task<Guid> CreateConnectorAsync(ConnectorInputModel connectorInputModel, string iamUserId, CancellationToken cancellationToken);

	/// <summary>
	/// Add a managed connector to persistence layer and calls the sd factory service with connector parameters.
	/// </summary>
	/// <param name="connectorInputModel">Connector parameters for creation.</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken"></param>
	/// <returns>View model of created connector.</returns>
	Task<Guid> CreateManagedConnectorAsync(ManagedConnectorInputModel connectorInputModel, string iamUserId, CancellationToken cancellationToken);

	/// <summary>
	/// Remove a connector from persistence layer by id.
	/// </summary>
	/// <param name="connectorId">ID of the connector to be deleted.</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken">Cancellation Token</param>
	Task DeleteConnectorAsync(Guid connectorId, string iamUserId, CancellationToken cancellationToken);

	/// <summary>
	/// Retrieve connector end point along with bpns
	/// </summary>
	/// <param name="bpns"></param>
	/// <returns></returns>
	IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync(IEnumerable<string> bpns);

	/// <summary>
	/// Triggers the daps endpoint for the given trigger
	/// </summary>
	/// <param name="connectorId">Id of the connector the endpoint should get triggered for.</param>
	/// <param name="certificate">The certificate</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken"></param>
	/// <returns><c>true</c> if the call to daps was successful, otherwise <c>false</c>.</returns>
	Task<bool> TriggerDapsAsync(Guid connectorId, IFormFile certificate, string iamUserId, CancellationToken cancellationToken);

	/// <summary>
	/// Processes the clearinghouse self description
	/// </summary>
	/// <param name="data">The response data</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken">CancellationToken</param>
	Task ProcessClearinghouseSelfDescription(SelfDescriptionResponseData data, string iamUserId, CancellationToken cancellationToken);

	/// <summary>
	/// Update the connector url
	/// </summary>
	/// <param name="connectorId">Id of the connector</param>
	/// <param name="data">Update data for the connector</param>
	/// <param name="iamUserId">Id of the iam user</param>
	/// <param name="cancellationToken">CancellationToken</param>
	Task UpdateConnectorUrl(Guid connectorId, ConnectorUpdateRequest data, string iamUserId, CancellationToken cancellationToken);
}

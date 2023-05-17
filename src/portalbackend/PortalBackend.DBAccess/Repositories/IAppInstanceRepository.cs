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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing and creating app instances on persistence layer.
/// </summary>
public interface IAppInstanceRepository
{
	/// <summary>
	/// Creates an app instance 
	/// </summary>
	/// <param name="appId">Id of the app</param>
	/// <param name="iamClientId">Id of the iam client</param>
	/// <returns>The created App Instance</returns>
	AppInstance CreateAppInstance(Guid appId, Guid iamClientId);

	/// <summary>
	/// Removes the app instance
	/// </summary>
	/// <param name="appInstanceId">Id of the app instance</param>
	void RemoveAppInstance(Guid appInstanceId);

	void CreateAppInstanceAssignedServiceAccounts(IEnumerable<(Guid AppInstanceId, Guid CompanyServiceAccountId)> instanceAccounts);

	/// <summary>
	/// Checks whether there is already an app instance for the given offer
	/// </summary>
	/// <param name="offerId">Id of the offer</param>
	/// <returns><c>true</c> if an app instance exists, otherwise <c>false</c></returns>
	Task<bool> CheckInstanceExistsForOffer(Guid offerId);

	/// <summary>
	/// Gets the service accounts for an app instance
	/// </summary>
	/// <param name="appInstanceId">Id of the app instance</param>
	/// <returns>A list of the service account ids</returns>
	IAsyncEnumerable<Guid> GetAssignedServiceAccounts(Guid appInstanceId);

	/// <summary>
	/// Checks whether an appinstance has any assigned subscriptions
	/// </summary>
	/// <param name="appInstanceId">Id of the app instance</param>
	/// <returns><c>true</c> if subscriptions exists for this instance, otherwise <c>false</c></returns>
	Task<bool> CheckInstanceHasAssignedSubscriptions(Guid appInstanceId);

	/// <summary>
	/// Removes the app instance assigned service accounts
	/// </summary>
	/// <param name="appInstanceId">id of the app instance</param>
	/// <param name="serviceAccountIds">Ids of the assigned service accounts</param>
	void RemoveAppInstanceAssignedServiceAccounts(Guid appInstanceId, IEnumerable<Guid> serviceAccountIds);
}

/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;

public interface ICustodianBusinessLogic
{
	/// <summary>
	/// Gets the wallet data for the given application
	/// </summary>
	/// <param name="applicationId">Application to get the wallet data for</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns>Returns the wallet data if existing or null</returns>
	Task<WalletData?> GetWalletByBpnAsync(Guid applicationId, CancellationToken cancellationToken);

	/// <summary>
	/// Creates the wallet for the company of the application
	/// </summary>
	/// <param name="context">Context for the identity wallet creation.</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns>Returns the checklist data</returns>
	Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateIdentityWalletAsync(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken);
}

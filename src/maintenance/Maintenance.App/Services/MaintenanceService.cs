/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.ProcessIdentity;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

/// <summary>
/// Service to
/// 1. delete the pending and inactive documents as well as the depending on consents from the database
/// 2. schedule clearinghouse process steps where the END_CLEARINGHOUSE is in todo for a specific duration 
/// </summary>
public class MaintenanceService(IServiceScopeFactory serviceScopeFactory)
{
    /// <summary>
    /// executes the logic
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var processIdentityDataDetermination = scope.ServiceProvider.GetRequiredService<IProcessIdentityDataDetermination>();
        //call processIdentityDataDetermination.GetIdentityData() once to initialize IdentityService IdentityData for synchronous use:
        await processIdentityDataDetermination.GetIdentityData().ConfigureAwait(ConfigureAwaitOptions.None);

        var batchDeleteBusinessLogic = scope.ServiceProvider.GetRequiredService<IBatchDeleteService>();
        var clearinghouseBusinessLogic = scope.ServiceProvider.GetRequiredService<IClearinghouseBusinessLogic>();

        if (!cancellationToken.IsCancellationRequested)
        {
            await batchDeleteBusinessLogic.CleanupDocuments(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await clearinghouseBusinessLogic.CheckEndClearinghouseProcesses(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}

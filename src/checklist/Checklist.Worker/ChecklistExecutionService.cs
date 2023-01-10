/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;

/// <summary>
/// Service that checks if there are open/pending tasks of a checklist and executes them.
/// </summary>
public class ChecklistExecutionService : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ChecklistExecutionService> _logger;
    private readonly int _workerBatchSize;

    /// <summary>
    /// Creates a new instance of <see cref="ChecklistExecutionService"/>
    /// </summary>
    /// <param name="applicationLifetime">Application lifetime</param>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="config">the configuration</param>
    public ChecklistExecutionService(
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ChecklistExecutionService> logger,
        IConfiguration config)
    {
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _workerBatchSize = config.GetValue<int>("WorkerBatchSize");
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var portalRepositories = scope.ServiceProvider.GetRequiredService<IPortalRepositories>();
            
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var checklistService = scope.ServiceProvider.GetRequiredService<IChecklistService>();
                var checklistEntryData = await portalRepositories.GetInstance<IApplicationChecklistRepository>().GetChecklistDataGroupedByApplicationId(_workerBatchSize).ToListAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Found {Count} application checklists to process", checklistEntryData.Count);
                foreach (var entryData in checklistEntryData)
                {
                    await checklistService.ProcessChecklist(entryData.ApplicationId, entryData.ChecklistEntries, stoppingToken).ConfigureAwait(false);
                }
                _logger.LogInformation("Processed checklist items");
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Checklist processing failed with following Exception {ExceptionMessage}", ex.Message);
            }
        }

        _applicationLifetime.StopApplication();
    }
}

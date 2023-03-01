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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Library;

/// <summary>
/// Service that reads all open/pending processSteps of a checklist and triggers their execution.
/// </summary>
public class ProcessExecutionService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessExecutionService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="ProcessExecutionService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    public ProcessExecutionService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessExecutionService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles the checklist processing
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var processServiceScope = _serviceScopeFactory.CreateScope();
            var executorRepositories = processServiceScope.ServiceProvider.GetRequiredService<IPortalRepositories>();
            var processExecutor = processServiceScope.ServiceProvider.GetRequiredService<IProcessExecutor>();

            using var outerLoopScope = _serviceScopeFactory.CreateScope();
            var outerLoopRepositories = outerLoopScope.ServiceProvider.GetRequiredService<IPortalRepositories>();

            var activeProcesses = outerLoopRepositories.GetInstance<IProcessStepRepository>().GetActiveProcesses(processExecutor.GetRegisteredProcessTypeIds());
            await foreach (var (processId, processTypeId) in activeProcesses.WithCancellation(stoppingToken).ConfigureAwait(false))
            {
                await foreach (var modified in processExecutor.ExecuteProcess(processId, processTypeId, stoppingToken).WithCancellation(stoppingToken).ConfigureAwait(false))
                {
                    if (modified)
                    {
                        await executorRepositories.SaveAsync().ConfigureAwait(false);
                    }

                    executorRepositories.Clear();
                }
                _logger.LogInformation("finished processing process {processId} type {processType}", processId, processTypeId);
            }
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            _logger.LogError("processing failed with following Exception {ExceptionMessage}", ex.Message);
        }
    }
}

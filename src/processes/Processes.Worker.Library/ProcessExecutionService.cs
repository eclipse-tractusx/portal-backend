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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;

/// <summary>
/// Service that reads all open/pending processSteps of a checklist and triggers their execution.
/// </summary>
public class ProcessExecutionService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ProcessExecutionServiceSettings _settings;
    private readonly ILogger<ProcessExecutionService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="ProcessExecutionService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    public ProcessExecutionService(
        IServiceScopeFactory serviceScopeFactory,
        IDateTimeProvider dateTimeProvider,
        IOptions<ProcessExecutionServiceSettings> options,
        ILogger<ProcessExecutionService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _dateTimeProvider = dateTimeProvider;
        _settings = options.Value;
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

            var lockExpiryTime = new TimeSpan(_settings.LockExpirySeconds * 10000000L);
            var activeProcesses = outerLoopRepositories.GetInstance<IProcessStepRepository>().GetActiveProcesses(processExecutor.GetRegisteredProcessTypeIds(), processExecutor.GetExecutableStepTypeIds(), _dateTimeProvider.OffsetNow);
            await foreach (var process in activeProcesses.WithCancellation(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    if (process.IsLocked())
                    {
                        _logger.LogInformation("skipping locked process {processId} type {processType}, lock expires at {lockExpireDate}", process.Id, process.ProcessTypeId, process.LockExpiryDate);
                        continue;
                    }
                    _logger.LogInformation("start processing process {processId} type {processType}", process.Id, process.ProcessTypeId);

                    bool EnsureLock()
                    {
                        if (process.IsLocked())
                        {
                            return false;
                        }
                        var isLocked = process.TryLock(_dateTimeProvider.OffsetNow.Add(lockExpiryTime));
                        if (!isLocked)
                        {
                            throw new UnexpectedConditionException("process TryLock should never fail here");
                        }
                        return true;
                    }

                    bool UpdateVersion()
                    {
                        if (!process.IsLocked())
                        {
                            process.UpdateVersion();
                        }
                        return true;
                    }

                    await foreach (var executionResult in processExecutor.ExecuteProcess(process.Id, process.ProcessTypeId, stoppingToken).WithCancellation(stoppingToken).ConfigureAwait(false))
                    {
                        if (executionResult switch
                        {
                            IProcessExecutor.ProcessExecutionResult.LockRequested => EnsureLock(),
                            IProcessExecutor.ProcessExecutionResult.SaveRequested => UpdateVersion(),
                            _ => false
                        })
                        {
                            await executorRepositories.SaveAsync().ConfigureAwait(false);
                        }
                        executorRepositories.Clear();
                    }

                    if (process.ReleaseLock())
                    {
                        await executorRepositories.SaveAsync().ConfigureAwait(false);
                        executorRepositories.Clear();
                    }
                    _logger.LogInformation("finished processing process {processId}", process.Id);
                }
                catch(Exception ex) when (ex is not SystemException)
                {
                    _logger.LogInformation(ex, "error processing process {processId} type {processType}: {message}", process.Id, process.ProcessTypeId, ex.Message);
                    executorRepositories.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            _logger.LogError(ex, "processing failed with following Exception {ExceptionMessage}", ex.Message);
        }
    }
}

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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;

/// <summary>
/// Service that checks if there are open/pending tasks of a checklist and executes them.
/// </summary>
public class ChecklistExecutionService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ChecklistExecutionService> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="ChecklistExecutionService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    public ChecklistExecutionService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ChecklistExecutionService> logger)
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
        using var outerLoopScope = _serviceScopeFactory.CreateScope();
        var outerLoopRepositories = outerLoopScope.ServiceProvider.GetRequiredService<IPortalRepositories>();

        using var checklistServiceScope = outerLoopScope.ServiceProvider.CreateScope();
        var checklistService = checklistServiceScope.ServiceProvider.GetRequiredService<IChecklistService>();
        var applicationActivation = checklistServiceScope.ServiceProvider.GetRequiredService<IApplicationActivationService>();
        var checklistCreationService = checklistServiceScope.ServiceProvider.GetRequiredService<IChecklistCreationService>();
        var checklistRepositories = checklistServiceScope.ServiceProvider.GetRequiredService<IPortalRepositories>();

        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var checklistEntryData = outerLoopRepositories.GetInstance<IApplicationChecklistRepository>().GetChecklistDataOrderedByApplicationId().PreSortedGroupBy(x => x.ApplicationId).ConfigureAwait(false);
                await foreach (var entryData in checklistEntryData.WithCancellation(stoppingToken).ConfigureAwait(false))
                {
                    var applicationId = entryData.Key;
                    var checklistEntries = await HandleChecklistProcessing(entryData, checklistCreationService, applicationId, checklistService, checklistRepositories, stoppingToken).ToListAsync(stoppingToken).ConfigureAwait(false);
                    await HandleApplicationActivation(checklistEntries, applicationActivation, applicationId, checklistRepositories).ConfigureAwait(false);
                }
                _logger.LogInformation("Processed checklist items");
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Checklist processing failed with following Exception {ExceptionMessage}", ex.Message);
            }
        }
    }

    private static async IAsyncEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)> HandleChecklistProcessing(
        IEnumerable<(Guid ApplicationId, ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> entryData,
        IChecklistCreationService checklistCreationService,
        Guid applicationId,
        IChecklistService checklistService,
        IPortalRepositories checklistRepositories,
        [EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var existingChecklistTypes = entryData.Select(e => e.TypeId);
        var checklistEntries = entryData.Select(e => (e.TypeId, e.StatusId))
            .ToList();
        if (Enum.GetValues<ApplicationChecklistEntryTypeId>().Length != existingChecklistTypes.Count())
        {
            var newlyCreatedEntries = await checklistCreationService
                .CreateMissingChecklistItems(applicationId, existingChecklistTypes).ConfigureAwait(false);
            checklistEntries.AddRange(newlyCreatedEntries);
            await checklistRepositories.SaveAsync().ConfigureAwait(false);
            checklistRepositories.Clear();
        }

        await foreach (var (typeId, statusId, processed) in checklistService.ProcessChecklist(applicationId, checklistEntries, stoppingToken).ConfigureAwait(false))
        {
            if (processed)
            {
                await checklistRepositories.SaveAsync().ConfigureAwait(false);
                checklistRepositories.Clear();
            }
            yield return (typeId, statusId);
        }
    }

    private async Task HandleApplicationActivation(IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> checklistEntries,
        IApplicationActivationService applicationActivation, Guid applicationId, IPortalRepositories checklistRepositories)
    {
        if (checklistEntries.All(x => x.StatusId == ApplicationChecklistEntryStatusId.DONE))
        {
            try
            {
                await applicationActivation.HandleApplicationActivation(applicationId).ConfigureAwait(false);
                await checklistRepositories.SaveAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Application activation for application {ApplicationId} failed with error {ErrorMessage}",
                    applicationId, ex.ToString());
            }
            checklistRepositories.Clear();
        }
    }
}

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
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;

/// <summary>
/// Service that reads all open/pending processSteps of a checklist and triggers their execution.
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

    private static readonly IEnumerable<ProcessStepTypeId> _automaticProcessStepTypeIds = new [] {
        ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL,
        ProcessStepTypeId.CREATE_IDENTITY_WALLET,
        ProcessStepTypeId.START_CLEARING_HOUSE,
        ProcessStepTypeId.CREATE_SELF_DESCRIPTION_LP,
        ProcessStepTypeId.ACTIVATE_APPLICATION,
    }.ToImmutableArray();

    /// <summary>
    /// Handles the checklist processing
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var outerLoopScope = _serviceScopeFactory.CreateScope();
        var outerLoopRepositories = outerLoopScope.ServiceProvider.GetRequiredService<IPortalRepositories>();

        using var checklistServiceScope = outerLoopScope.ServiceProvider.CreateScope();
        var checklistProcessor = checklistServiceScope.ServiceProvider.GetRequiredService<IChecklistProcessor>();
        var checklistCreationService = checklistServiceScope.ServiceProvider.GetRequiredService<IChecklistCreationService>();
        var checklistRepositories = checklistServiceScope.ServiceProvider.GetRequiredService<IPortalRepositories>();

        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var checklistEntryData = outerLoopRepositories.GetInstance<IApplicationChecklistRepository>().GetChecklistProcessStepData();
                await foreach (var entryData in checklistEntryData.WithCancellation(stoppingToken).ConfigureAwait(false))
                {
                    var checklist = await HandleChecklistProcessing(entryData, checklistCreationService, checklistProcessor, checklistRepositories, stoppingToken).ConfigureAwait(false);
                    _logger.LogInformation("Processed application {applicationId} checklist. Result: {result}", entryData.ApplicationId, checklist);
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

    private static async Task<IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)>> HandleChecklistProcessing(
        (Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> Checklist, IEnumerable<ProcessStep> ProcessSteps) entryData,
        IChecklistCreationService checklistCreationService,
        IChecklistProcessor checklistProcessor,
        IPortalRepositories checklistRepositories,
        CancellationToken stoppingToken)
    {
        var (applicationId, checklistEntries, processSteps) = entryData;
        if (Enum.GetValues<ApplicationChecklistEntryTypeId>().Length != checklistEntries.Count())
        {
            var createdEntries = (await checklistCreationService
                .CreateMissingChecklistItems(applicationId, checklistEntries.Select(entry => entry.TypeId)).ConfigureAwait(false)).ToList();
            checklistEntries = checklistEntries.Concat(createdEntries);

            var newSteps = checklistCreationService
                .CreateInitialProcessSteps(applicationId, createdEntries).ToList();
            processSteps = processSteps.Concat(newSteps.IntersectBy(_automaticProcessStepTypeIds, processStep => processStep.ProcessStepTypeId));

            await checklistRepositories.SaveAsync().ConfigureAwait(false);
            checklistRepositories.Clear();
        }
        var checklist = checklistEntries.ToDictionary(entry => entry.TypeId, entry => entry.StatusId);

        await foreach (var (typeId, statusId) in checklistProcessor.ProcessChecklist(applicationId, checklistEntries, processSteps, stoppingToken).WithCancellation(stoppingToken).ConfigureAwait(false))
        {
            await checklistRepositories.SaveAsync().ConfigureAwait(false);
            checklistRepositories.Clear();
            checklist[typeId] = statusId;
        }
        return checklist.Select(entry => (entry.Key, entry.Value));
    }
}

/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.SelfDescriptionCreation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.SelfDescriptionCreation.Executor;

public class SdCreationProcessTypeExecutor(IPortalRepositories portalRepositories, ISdFactoryService sdFactoryService, IOptions<SelfDescriptionProcessSettings> options)
    : IProcessTypeExecutor
{
    private readonly SelfDescriptionProcessSettings _settings = options.Value;
    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps = [ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION];

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.SELF_DESCRIPTION_CREATION;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        return await Task.FromResult(new IProcessTypeExecutor.InitializationResult(false, null));
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION => await CreateSdDocumentForConnector(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION => await CreateSdDocumentForCompany(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None),
                _ => throw new UnexpectedConditionException($"unexpected processStepTypeId {processStepTypeId} for process {ProcessTypeId.SELF_DESCRIPTION_CREATION}")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? NextStepTypeIds, ProcessStepStatusId StepStatusId, bool Modified, string? ProcessMessage)> CreateSdDocumentForCompany(CancellationToken cancellationToken)
    {
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        var companyInformation = companyRepository.GetNextCompaniesWithMissingSelfDescription();
        await using var enumerator = companyInformation.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var (id, uniqueIdentifiers, businessPartnerNumber, countryCode) = enumerator.Current;
            if (string.IsNullOrWhiteSpace(businessPartnerNumber))
            {
                throw new ConflictException("BusinessPartnerNumber should never be null here.");
            }

            await sdFactoryService.RegisterSelfDescriptionAsync(id, uniqueIdentifiers, countryCode, businessPartnerNumber, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? Enumerable.Repeat(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, 1) // in case there are further companies or connectors need self description documents
                : null;
            return (nextStepTypeIds, ProcessStepStatusId.DONE, true, $"create self description document started for connector {id}");
        }

        return (null, ProcessStepStatusId.DONE, false, "no companies without self description document");
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? NextStepTypeIds, ProcessStepStatusId StepStatusId, bool Modified, string? ProcessMessage)> CreateSdDocumentForConnector(CancellationToken cancellationToken)
    {
        var connectorsRepository = portalRepositories.GetInstance<IConnectorsRepository>();
        var connectorInformation = connectorsRepository.GetNextConnectorsWithMissingSelfDescription();
        await using var enumerator = connectorInformation.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var (id, businessPartnerNumber, selfDescriptionDocumentId) = enumerator.Current;
            if (string.IsNullOrWhiteSpace(businessPartnerNumber))
            {
                throw new ConflictException("BusinessPartnerNumber should never be null here.");
            }

            var selfDescriptionDocumentUrl = $"{_settings.SelfDescriptionDocumentUrl}/{selfDescriptionDocumentId}";
            await sdFactoryService.RegisterConnectorAsync(id, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? Enumerable.Repeat(ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION, 1) // in case there are further companies or connectors need self description documents
                : null;
            return (nextStepTypeIds, ProcessStepStatusId.DONE, true, $"create self description document started for connector {id}");
        }

        return (null, ProcessStepStatusId.DONE, false, "no connectors without self description document");
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId stepTypeId) =>
        (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(stepTypeId == ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION ?
            ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_COMPANY_CREATION :
            ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_CONNECTOR_CREATION, 1));
}

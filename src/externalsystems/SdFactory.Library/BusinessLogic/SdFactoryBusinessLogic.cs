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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;

public class SdFactoryBusinessLogic(
    ISdFactoryService sdFactoryService,
    IPortalRepositories portalRepositories,
    IApplicationChecklistService checklistService,
    IOptions<SdFactorySettings> options)
    : ISdFactoryBusinessLogic
{
    private readonly SdFactorySettings _settings = options.Value;

    /// <inheritdoc />
    public Task RegisterConnectorAsync(
        Guid connectorId,
        string selfDescriptionDocumentUrl,
        string businessPartnerNumber,
        CancellationToken cancellationToken) =>
        sdFactoryService.RegisterConnectorAsync(connectorId, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken);

    /// <inheritdoc />
    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> StartSelfDescriptionRegistration(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (_settings.ClearinghouseConnectDisabled)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.SKIPPED,
                entry =>
                {
                    entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.SKIPPED;
                    entry.Comment = "Self description was skipped due to clearinghouse trigger is disabled";
                },
                [ProcessStepTypeId.ASSIGN_INITIAL_ROLES],
                null,
                true,
                "Self description was skipped due to clearinghouse trigger is disabled"
            );
        }

        await RegisterSelfDescriptionInternalAsync(context.ApplicationId, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            [ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE],
            null,
            true,
            null
        );
    }

    private async Task RegisterSelfDescriptionInternalAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(applicationId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        var (companyId, legalName, businessPartnerNumber, countryCode, region, uniqueIdentifiers) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException(
                $"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(region))
        {
            throw new ConflictException(
                $"CountryCode or Region for CompanyApplications {applicationId} and Company {companyId} is empty. Expected value: DE-NW and Current value: {countryCode}-{region}");
        }

        await sdFactoryService
            .RegisterSelfDescriptionAsync(applicationId, legalName, uniqueIdentifiers, countryCode, region, businessPartnerNumber, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task ProcessFinishSelfDescriptionLpForApplication(SelfDescriptionResponseData data, Guid companyId, CancellationToken cancellationToken)
    {
        var confirm = ValidateConfirmationData(data);
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                data.ExternalId,
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_LP_RESPONSE,
                processStepTypeIds: [ProcessStepTypeId.START_SELF_DESCRIPTION_LP])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (confirm)
        {
            var documentId = await ProcessAndCreateDocument(SdFactoryResponseModelTitle.LegalPerson, data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null,
                c => { c.SelfDescriptionDocumentId = documentId; });
        }

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId =
                    confirm
                        ? ApplicationChecklistEntryStatusId.DONE
                        : ApplicationChecklistEntryStatusId.FAILED;
                item.Comment = data.Message;
            },
            confirm ? new[] { ProcessStepTypeId.ASSIGN_INITIAL_ROLES } : null);
    }

    /// <inheritdoc />
    public async Task ProcessFinishSelfDescriptionLpForConnector(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        var connectorsRepository = portalRepositories.GetInstance<IConnectorsRepository>();
        var result = ValidateConfirmationData(data);
        if (result)
        {
            var documentId = await ProcessAndCreateDocument(SdFactoryResponseModelTitle.Connector, data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            connectorsRepository.AttachAndModifyConnector(data.ExternalId, null, con =>
            {
                con.SelfDescriptionDocumentId = documentId;
                con.StatusId = ConnectorStatusId.ACTIVE;
                con.DateLastChanged = DateTimeOffset.UtcNow;
            });
        }
        // If the message contains the outdated legal person error code,
        // the connector is activated but no document is created
        else if (IsOutdatedLegalPerson(data.Message, _settings.ConnectorAllowSdDocumentSkipErrorCode))
        {
            connectorsRepository.AttachAndModifyConnector(data.ExternalId, null, con =>
            {
                con.StatusId = ConnectorStatusId.ACTIVE;
                con.SelfDescriptionMessage = data.Message!;
                con.DateLastChanged = DateTimeOffset.UtcNow;
                con.SdSkippedDate = DateTimeOffset.UtcNow;
            });
        }
        else
        {
            connectorsRepository.AttachAndModifyConnector(data.ExternalId, null, con =>
            {
                con.SelfDescriptionMessage = data.Message!;
                con.DateLastChanged = DateTimeOffset.UtcNow;
            });
        }

        var processData = await connectorsRepository.GetProcessDataForConnectorId(data.ExternalId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (processData != null)
        {
            HandleSdCreationProcess(processData, data, ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE, ProcessStepTypeId.RETRIGGER_AWAIT_SELF_DESCRIPTION_CONNECTOR_RESPONSE, SdFactoryRequestModelSdType.ServiceOffering);
        }
    }

    private static bool IsOutdatedLegalPerson(string? message, string? expectedErrorCode) => message != null && expectedErrorCode != null && message.Contains(expectedErrorCode);

    private void HandleSdCreationProcess(VerifyProcessData<ProcessTypeId, ProcessStepTypeId> processData, SelfDescriptionResponseData data, ProcessStepTypeId processStepTypeId, ProcessStepTypeId retriggerProcessStepTypeId, SdFactoryRequestModelSdType sdType)
    {
        var context = processData.CreateManualProcessData(processStepTypeId, portalRepositories, () => $"externalId {data.ExternalId}");
        if (data.Status == SelfDescriptionStatus.Confirm)
        {
            context.FinalizeProcessStep();
        }
        else if (sdType == SdFactoryRequestModelSdType.ServiceOffering && IsOutdatedLegalPerson(data.Message, _settings.ConnectorAllowSdDocumentSkipErrorCode))
        {
            context.ScheduleProcessSteps([ProcessStepTypeId.RETRIGGER_CONNECTOR_SELF_DESCRIPTION_WITH_OUTDATED_LEGAL_PERSON]);
            context.FinalizeProcessStep();
        }
        else
        {
            context.ScheduleProcessSteps([retriggerProcessStepTypeId]);
            context.FailProcessStep(data.Message);
        }
    }

    public async Task ProcessFinishSelfDescriptionLpForCompany(SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        if (data.Status == SelfDescriptionStatus.Confirm)
        {
            var documentId = await ProcessAndCreateDocument(SdFactoryResponseModelTitle.LegalPerson, data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            companyRepository.AttachAndModifyCompany(data.ExternalId, null, c => { c.SelfDescriptionDocumentId = documentId; });
        }

        var processData = await companyRepository.GetProcessDataForCompanyIdId(data.ExternalId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (processData != null)
        {
            HandleSdCreationProcess(processData, data, ProcessStepTypeId.AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE, ProcessStepTypeId.RETRIGGER_AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE, SdFactoryRequestModelSdType.LegalParticipant);
        }
    }

    private static bool ValidateConfirmationData(SelfDescriptionResponseData data)
    {
        var confirm = data.Status == SelfDescriptionStatus.Confirm;
        switch (confirm)
        {
            case false when string.IsNullOrEmpty(data.Message):
                throw new ConflictException("Please provide a message");
            case true when data.Content == null:
                throw new ConflictException("Please provide a selfDescriptionDocument");
        }

        return confirm;
    }

    private async Task<Guid> ProcessAndCreateDocument(SdFactoryResponseModelTitle title, SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });
        var jsonDocument = JsonDocument.Parse(data.Content!);
        jsonDocument.WriteTo(writer);

        await writer.FlushAsync(cancellationToken);
        var documentContent = ms.ToArray();
        var hash = SHA512.HashData(documentContent);

        var document = portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(
            $"SelfDescription_{title}.json",
            documentContent,
            hash,
            MediaTypeId.JSON,
            DocumentTypeId.SELF_DESCRIPTION,
            documentContent.Length,
            doc => { doc.DocumentStatusId = DocumentStatusId.LOCKED; });
        return document.Id;
    }
}

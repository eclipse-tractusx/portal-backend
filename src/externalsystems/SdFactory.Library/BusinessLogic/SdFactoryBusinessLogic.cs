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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;

public class SdFactoryBusinessLogic : ISdFactoryBusinessLogic
{
    private readonly ISdFactoryService _sdFactoryService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationChecklistService _checklistService;

    public SdFactoryBusinessLogic(ISdFactoryService sdFactoryService, IPortalRepositories portalRepositories,
        IApplicationChecklistService checklistService)
    {
        _sdFactoryService = sdFactoryService;
        _portalRepositories = portalRepositories;
        _checklistService = checklistService;
    }

    /// <inheritdoc />
    public Task RegisterConnectorAsync(
        Guid connectorId,
        string selfDescriptionDocumentUrl,
        string businessPartnerNumber,
        CancellationToken cancellationToken) =>
        _sdFactoryService.RegisterConnectorAsync(connectorId, selfDescriptionDocumentUrl, businessPartnerNumber, cancellationToken);

    /// <inheritdoc />
    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> StartSelfDescriptionRegistration(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        await RegisterSelfDescriptionInternalAsync(context.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            new[] { ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP },
            null,
            true,
            null
        );
    }

    private async Task RegisterSelfDescriptionInternalAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(applicationId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        var (companyId, businessPartnerNumber, countryCode, uniqueIdentifiers) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException(
                $"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }

        await _sdFactoryService
            .RegisterSelfDescriptionAsync(applicationId, uniqueIdentifiers, countryCode, businessPartnerNumber, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ProcessFinishSelfDescriptionLpForApplication(SelfDescriptionResponseData data, Guid companyId, CancellationToken cancellationToken)
    {
        var confirm = ValidateData(data, out var validated);
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                data.ExternalId,
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
                new[] { ApplicationChecklistEntryStatusId.IN_PROGRESS },
                ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP,
                processStepTypeIds: new[] { ProcessStepTypeId.START_SELF_DESCRIPTION_LP })
            .ConfigureAwait(false);

        if (confirm)
        {
            var documentId = await ProcessDocument(SdFactoryResponseModelTitle.LegalPerson, validated.Content, cancellationToken).ConfigureAwait(false);
            _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null,
                c => { c.SelfDescriptionDocumentId = documentId; });
        }

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
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
            confirm ? new[] { ProcessStepTypeId.ACTIVATE_APPLICATION } : null);
    }

    /// <inheritdoc />
    public async Task ProcessFinishSelfDescriptionLpForConnector(SelfDescriptionResponseData data, Guid companyUserId, CancellationToken cancellationToken)
    {
        var confirm = ValidateData(data, out var validated);
        Guid? documentId = null;
        if (confirm)
        {
            documentId = await ProcessDocument(SdFactoryResponseModelTitle.Connector, validated.Content, cancellationToken).ConfigureAwait(false);
        }
        _portalRepositories.GetInstance<IConnectorsRepository>().AttachAndModifyConnector(data.ExternalId, null, con =>
        {
            if (documentId != null)
            {
                con.SelfDescriptionDocumentId = documentId;
            }

            if (!confirm)
            {
                con.SelfDescriptionMessage = validated.Message;
            }

            con.DateLastChanged = DateTimeOffset.UtcNow;
        });
    }

    private sealed class ValidatedResponseData
    {
        private readonly bool _confirm;
        private readonly SelfDescriptionResponseData _data;

        public ValidatedResponseData(SelfDescriptionResponseData data)
        {
            _confirm = data.Status == SelfDescriptionStatus.Confirm;
            switch (_confirm)
            {
                case false when string.IsNullOrEmpty(data.Message):
                    throw new ConflictException("Please provide a messsage");
                case true when data.Content == null:
                    throw new ConflictException("Please provide a selfDescriptionDocument");
            }
            _data = data;
        }

        public bool Confirm
        {
            get => _confirm;
        }

        public string Message
        {
            get => _data.Message == null || _confirm
                ? throw new InvalidOperationException("Message may only be called when Confirm is false")
                : _data.Message;
        }

        public JsonDocument Content
        {
            get => _data.Content == null || !_confirm
                ? throw new InvalidOperationException("Content may only be called when Confirm is true")
                : _data.Content;
        }
    }

    private static bool ValidateData(SelfDescriptionResponseData data, out ValidatedResponseData validated)
    {
        validated = new(data);
        return validated.Confirm;
    }

    private async Task<Guid> ProcessDocument(SdFactoryResponseModelTitle title, JsonDocument content, CancellationToken cancellationToken)
    {
        using var sha512Hash = SHA512.Create();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true });
        content.WriteTo(writer);

        await writer.FlushAsync(cancellationToken);
        var documentContent = ms.ToArray();
        var hash = sha512Hash.ComputeHash(documentContent);

        var document = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(
            $"SelfDescription_{title}.json",
            documentContent,
            hash,
            MediaTypeId.JSON,
            DocumentTypeId.SELF_DESCRIPTION,
            doc => { doc.DocumentStatusId = DocumentStatusId.LOCKED; });
        return document.Id;
    }
}

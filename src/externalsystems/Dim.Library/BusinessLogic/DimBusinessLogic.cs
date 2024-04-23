/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Json.Schema;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Reflection;
using System.Text.Json;
using System.Web;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;

public class DimBusinessLogic : IDimBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IDimService _dimService;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DimSettings _settings;

    public DimBusinessLogic(IPortalRepositories portalRepositories, IDimService dimService, IApplicationChecklistService checklistService, IDateTimeProvider dateTimeProvider, IOptions<DimSettings> options)
    {
        _portalRepositories = portalRepositories;
        _dimService = dimService;
        _checklistService = checklistService;
        _dateTimeProvider = dateTimeProvider;
        _settings = options.Value;
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateDimWalletAsync(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (context.Checklist[ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER] == ApplicationChecklistEntryStatusId.FAILED || context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == ApplicationChecklistEntryStatusId.FAILED)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.SKIPPED,
                checklistEntry => checklistEntry.Comment = $"processStep CREATE_IDENTITY_WALLET skipped as entries BUSINESS_PARTNER_NUMBER and REGISTRATION_VERIFICATION have status {context.Checklist[ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER]} and {context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION]}",
                null,
                null,
                true,
                null);
        }

        if (context.Checklist[ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER] == ApplicationChecklistEntryStatusId.DONE && context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == ApplicationChecklistEntryStatusId.DONE)
        {
            await CreateWalletInternal(context.ApplicationId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.DONE,
                checklist =>
                    {
                        checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
                    },
                [ProcessStepTypeId.AWAIT_DIM_RESPONSE],
                null,
                true,
                null);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, null);
    }

    private async Task CreateWalletInternal(Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyAndApplicationDetailsForCreateWalletAsync(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        var (companyId, companyName, businessPartnerNumber) = result;
        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }

        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(result.CompanyId, c => c.DidDocumentLocation = null, c => c.DidDocumentLocation = $"{_settings.DidDocumentBaseLocation}/{HttpUtility.UrlEncode(businessPartnerNumber)}/did.json");
        await _dimService.CreateWalletAsync(companyName, businessPartnerNumber, $"{_settings.DidDocumentBaseLocation}/{businessPartnerNumber}/did.json", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task ProcessDimResponse(string bpn, DimWalletData data, CancellationToken cancellationToken)
    {
        var (exists, companyId, companyApplicationStatusIds) = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyIdByBpn(bpn).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"No company found for bpn {bpn}");
        }

        if (companyApplicationStatusIds.Count() != 1)
        {
            throw new ConflictException($"There must be exactly one company application in state {CompanyApplicationStatusId.SUBMITTED}");
        }

        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                companyApplicationStatusIds.Single(),
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                ProcessStepTypeId.AWAIT_DIM_RESPONSE,
                processStepTypeIds: [ProcessStepTypeId.VALIDATE_DID_DOCUMENT])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!ValidateDidFormat(data.Did, bpn))
        {
            _checklistService.FinalizeChecklistEntryAndProcessSteps(
                context,
                null,
                item =>
                {
                    item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                    item.Comment = "The did did not match the expected format";
                },
                null);
            return;
        }

        if (!await ValidateSchema(data.DidDocument, cancellationToken))
        {
            _checklistService.FinalizeChecklistEntryAndProcessSteps(
                context,
                null,
                item =>
                {
                    item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                    item.Comment = "The did document did not match the expected schema";
                },
                null);
            return;
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncryptionConfigIndex) ?? throw new ConfigurationException($"encryptionConfigIndex {_settings.EncryptionConfigIndex} is not configured");
        var (secret, initializationVector) = CryptoHelper.Encrypt(data.AuthenticationDetails.ClientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        _portalRepositories.GetInstance<ICompanyRepository>().CreateWalletData(companyId, data.Did, data.DidDocument, data.AuthenticationDetails.ClientId, secret, initializationVector, _settings.EncryptionConfigIndex, data.AuthenticationDetails.AuthenticationServiceUrl);

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            [ProcessStepTypeId.VALIDATE_DID_DOCUMENT]);
    }

    private bool ValidateDidFormat(string did, string bpn)
    {
        var expectedDid = $"did:web:{_settings.DidDocumentBaseLocation.Replace("https://", string.Empty).Replace("/", ":")}:{bpn}";
        return did.Equals(expectedDid, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> ValidateDidDocument(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (context.Checklist[ApplicationChecklistEntryTypeId.IDENTITY_WALLET] != ApplicationChecklistEntryStatusId.IN_PROGRESS)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.FAILED,
                checklistEntry => checklistEntry.Comment = $"processStep CREATE_IDENTITY_WALLET failed as entries IDENTITY_WALLET must have status {ApplicationChecklistEntryStatusId.IN_PROGRESS}",
                null,
                null,
                true,
                null);
        }

        var (result, dateCreated) = await ValidateDid(context.ApplicationId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.DONE,
                checklist =>
                {
                    checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
                },
                [ProcessStepTypeId.TRANSMIT_BPN_DID],
                null,
                true,
                null);
        }

        // Do stuff
        var maxTime = dateCreated.AddDays(_settings.MaxValidationTimeInDays);
        return _dateTimeProvider.OffsetNow > maxTime
            ? new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.FAILED,
                null,
                Enumerable.Repeat(ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT, 1),
                null,
                false,
                "The validation was aborted")
            : new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.TODO,
                null,
                null,
                null,
                false,
                null);
    }

    private async Task<(bool ValidationResult, DateTimeOffset DateCreated)> ValidateDid(Guid applicationId, CancellationToken cancellationToken)
    {
        var (exists, did, processStepsDateCreated) = await _portalRepositories.GetInstance<IApplicationRepository>().GetDidApplicationId(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(did))
        {
            throw new ConflictException("There must be a did set");
        }

        if (processStepsDateCreated.Count() != 1)
        {
            throw new ConflictException($"There must be excatly on active {ProcessStepTypeId.VALIDATE_DID_DOCUMENT}");
        }

        return (await _dimService.ValidateDid(did, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None), processStepsDateCreated.Single());
    }

    private static async Task<bool> ValidateSchema(JsonDocument content, CancellationToken cancellationToken)
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new UnexpectedConditionException("Assembly location must be set");

        var path = Path.Combine(location, "Schemas", "DidDocument.schema.json");
        var schemaJson = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        var schema = JsonSchema.FromText(schemaJson);
        SchemaRegistry.Global.Register(schema);
        var result = schema.Evaluate(content);
        return result.IsValid;
    }
}

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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using System.Collections.Immutable;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Executor;

public class MailingProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IMailingService mailingService,
    IOptions<MailingProcessCreationSettings> options)
    : IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>
{
    private readonly MailingProcessCreationSettings _settings = options.Value;

    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps = ImmutableArray.Create(ProcessStepTypeId.SEND_MAIL);
    private Guid _processId;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.MAILING;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        _processId = processId;
        return await Task.FromResult(new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult(false, null));
    }

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.SEND_MAIL => await SendMail().ConfigureAwait(ConfigureAwaitOptions.None),
                _ => throw new UnexpectedConditionException($"unexpected processStepTypeId {processStepTypeId} for process {ProcessTypeId.MAILING}")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? NextStepTypeIds, ProcessStepStatusId StepStatusId, bool Modified, string? ProcessMessage)> SendMail()
    {
        var mailingRepository = portalRepositories.GetInstance<IMailingInformationRepository>();
        var mailingInformation = mailingRepository.GetMailingInformationForProcess(_processId);
        await using var enumerator = mailingInformation.GetAsyncEnumerator();

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var (id, mail, template, mailParameters, initializationVector, encryptionMode) = enumerator.Current;

            await mailingService.SendMails(
                mail,
                DecryptMailParameters(mailParameters, initializationVector, encryptionMode),
                template).ConfigureAwait(ConfigureAwaitOptions.None);

            mailingRepository.AttachAndModifyMailingInformation(id,
                i =>
                {
                    i.MailingStatusId = MailingStatusId.PENDING;
                },
                i =>
                {
                    i.MailingStatusId = MailingStatusId.SENT;
                });
            var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? Enumerable.Repeat(ProcessStepTypeId.SEND_MAIL, 1) // in case there are further mailing information eligible to send the same step is created again
                : null;
            return (nextStepTypeIds, ProcessStepStatusId.DONE, true, $"send mail to {mail}");
        }

        return (null, ProcessStepStatusId.DONE, false, "no pending mails found");
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex) =>
        (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(ProcessStepTypeId.RETRIGGER_SEND_MAIL, 1));

    private IReadOnlyDictionary<string, string> DecryptMailParameters(byte[] mailParameters, byte[] initializationVector, int encryptionMode)
    {
        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == encryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {encryptionMode} is not configured");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(
            CryptoHelper.Decrypt(
                mailParameters,
                initializationVector,
                Convert.FromHexString(cryptoConfig.EncryptionKey),
                cryptoConfig.CipherMode,
                cryptoConfig.PaddingMode)) ?? throw new ConflictException("failed to deserialize mailParameters");
    }
}

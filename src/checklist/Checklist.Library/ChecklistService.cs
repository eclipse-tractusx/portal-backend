/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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

using System.Net;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public class ChecklistService : IChecklistService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmBusinessLogic _bpdmBusinessLogic;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;
    private readonly ILogger<IChecklistService> _logger;

    public ChecklistService(IPortalRepositories portalRepositories, IBpdmBusinessLogic bpdmBusinessLogic, ICustodianBusinessLogic custodianBusinessLogic, ILogger<IChecklistService> logger)
    {
        _portalRepositories = portalRepositories;
        _bpdmBusinessLogic = bpdmBusinessLogic;
        _custodianBusinessLogic = custodianBusinessLogic;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken)
    {
        await CheckCanRunStepAsync(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, new []{ ApplicationChecklistEntryStatusId.TO_DO, ApplicationChecklistEntryStatusId.FAILED }).ConfigureAwait(false);
        await _bpdmBusinessLogic.TriggerBpnDataPush(applicationId, iamUserId, cancellationToken).ConfigureAwait(false);
        
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ProcessChecklist(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> checklistEntries, CancellationToken cancellationToken)
    {
        var possibleSteps = GetNextPossibleTypesWithMatchingStatus(checklistEntries.ToDictionary(x => x.TypeId, x => x.StatusId), new[] { ApplicationChecklistEntryStatusId.TO_DO });
        _logger.LogInformation("Found {StepsCount} possible steps for application {ApplicationId}", possibleSteps.Count(), applicationId);
        if (possibleSteps.Contains(ApplicationChecklistEntryTypeId.IDENTITY_WALLET))
        {
            try
            {
                _logger.LogInformation("Executing wallet creation for application {ApplicationId}", applicationId);
                await CreateWalletAsync(applicationId, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Wallet successfully created for application {ApplicationId}", applicationId);
            }
            catch (Exception ex)
            {
                _portalRepositories.GetInstance<IApplicationChecklistRepository>()
                    .AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                        item => { 
                            item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                            item.Comment = ex.ToString(); 
                        });
                await _portalRepositories.SaveAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task<bool> CreateWalletAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var createdWallet = false;
        try
        {
            var message = await _custodianBusinessLogic.CreateWalletAsync(applicationId, cancellationToken).ConfigureAwait(false);
            _portalRepositories.GetInstance<IApplicationChecklistRepository>()
                .AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                    checklist =>
                    {
                        checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
                        checklist.Comment = message;
                    });
            createdWallet = true;
        }
        catch (ServiceException ex)
        {
            _portalRepositories.GetInstance<IApplicationChecklistRepository>()
                .AttachAndModifyApplicationChecklist(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                    checklist =>
                    {
                        checklist.ApplicationChecklistEntryStatusId = ex.StatusCode == HttpStatusCode.ServiceUnavailable ? ApplicationChecklistEntryStatusId.TO_DO : ApplicationChecklistEntryStatusId.FAILED;
                        checklist.Comment = ex.ToString();
                    });
        }
        
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return createdWallet;
    }

    /// <summary>
    /// Checks whether the given step can be executed
    /// </summary>
    /// <param name="applicationId">id of the application</param>
    /// <param name="step">the step that should be executed</param>
    /// <param name="allowedStatus"></param>
    /// <exception cref="ConflictException">Exception will be thrown if the possible steps don't contain the requested step.</exception>
    private async Task CheckCanRunStepAsync(Guid applicationId, ApplicationChecklistEntryTypeId step, IEnumerable<ApplicationChecklistEntryStatusId> allowedStatus)
    {
        var checklistData = await _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .GetChecklistDataAsync(applicationId).ToListAsync().ConfigureAwait(false);

        var possibleSteps = GetNextPossibleTypesWithMatchingStatus(checklistData.ToDictionary(x => x.TypeId, x => x.StatusId), allowedStatus);
        if (!possibleSteps.Contains(step))
        {
            throw new ConflictException($"{step} is not available as next step");
        }
    }

    private static IEnumerable<ApplicationChecklistEntryTypeId> GetNextPossibleTypesWithMatchingStatus(IDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId> currentStatus, IEnumerable<ApplicationChecklistEntryStatusId> checklistEntryStatusIds)
    {
        var possibleTypes = new List<ApplicationChecklistEntryTypeId>();
        if (currentStatus.TryGetValue(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, out var bpnStatus) && checklistEntryStatusIds.Contains(bpnStatus))
        {
            possibleTypes.Add(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER);
        }
        if (currentStatus.TryGetValue(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, out var registrationStatus) && checklistEntryStatusIds.Contains(registrationStatus))
        {
            possibleTypes.Add(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION);
        }
        if (currentStatus.TryGetValue(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, out var identityStatus) && checklistEntryStatusIds.Contains(identityStatus) && bpnStatus == ApplicationChecklistEntryStatusId.DONE && registrationStatus == ApplicationChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ApplicationChecklistEntryTypeId.IDENTITY_WALLET);
        }
        if (currentStatus.TryGetValue(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, out var clearingHouseStatus) && checklistEntryStatusIds.Contains(clearingHouseStatus) && identityStatus == ApplicationChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ApplicationChecklistEntryTypeId.CLEARING_HOUSE);
        }
        if (currentStatus.TryGetValue(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, out var selfDescriptionStatus) && checklistEntryStatusIds.Contains(selfDescriptionStatus) && clearingHouseStatus == ApplicationChecklistEntryStatusId.DONE)
        {
            possibleTypes.Add(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP);
        }

        return possibleTypes;
    }
}

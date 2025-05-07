/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;

public class OfferProviderBusinessLogic : IOfferProviderBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferProviderService _offerProviderService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly OfferProviderSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerProviderService">Access to the offer provider service</param>
    /// <param name="provisioningManager">Access to the provisioning manager</param>
    /// <param name="options">The options for the offer provider bl</param>
    public OfferProviderBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferProviderService offerProviderService,
        IProvisioningManager provisioningManager,
        IOptions<OfferProviderSettings> options)
    {
        _portalRepositories = portalRepositories;
        _offerProviderService = offerProviderService;
        _provisioningManager = provisioningManager;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProvider(Guid offerSubscriptionId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetTriggerProviderInformation(offerSubscriptionId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == null)
        {
            throw new NotFoundException($"OfferSubscription {offerSubscriptionId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(data.CompanyInformationData.Country))
        {
            throw new ConflictException("Country should be set for the company");
        }

        var triggerProvider = !string.IsNullOrWhiteSpace(data.AutoSetupUrl) && !data.IsSingleInstance;
        if (triggerProvider)
        {
            var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(data.AuthDetails!.EncryptionMode);
            var secret = cryptoHelper.Decrypt(
                data.AuthDetails!.ClientSecret,
                data.AuthDetails.InitializationVector);

            if (data.AuthDetails == null)
            {
                throw new ConflictException("Auth details in auto-setup should be configured for the company");
            }

            var autoSetupData = new OfferThirdPartyAutoSetupData(
                new OfferThirdPartyAutoSetupCustomerData(
                    data.CompanyInformationData.OrganizationName,
                    data.CompanyInformationData.Country,
                    data.CompanyInformationData.CompanyUserEmail),
                new OfferThirdPartyAutoSetupPropertyData(
                    data.CompanyInformationData.BusinessPartnerNumber,
                    offerSubscriptionId,
                    data.OfferId)
            );
            await _offerProviderService
                .TriggerOfferProvider(autoSetupData, data.AutoSetupUrl!, data.AuthDetails.AuthUrl, data.AuthDetails.ClientId, secret, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return (
            [
                data.IsSingleInstance ?
                    ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION :
                    ProcessStepTypeId.AWAIT_START_AUTOSETUP
            ],
            triggerProvider ? ProcessStepStatusId.DONE : ProcessStepStatusId.SKIPPED,
            true,
            null);
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProviderCallback(Guid offerSubscriptionId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetTriggerProviderCallbackInformation(offerSubscriptionId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (data == default)
        {
            throw new NotFoundException($"OfferSubscription {offerSubscriptionId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(data.CallbackUrl))
        {
            return (
                null,
                ProcessStepStatusId.SKIPPED,
                true,
                null);
        }

        if (data.Status != OfferSubscriptionStatusId.ACTIVE)
        {
            throw new ConflictException("offer subscription should be active");
        }

        if (string.IsNullOrWhiteSpace(data.ClientId))
        {
            throw new ConflictException("Client should be set");
        }
        if (data.AuthDetails == null)
        {
            throw new ConflictException("Auth details in auto-setup should be configured for the company");
        }

        IEnumerable<CallbackTechnicalUserInfoData>? technicalUsersInfoData = null;

        if (data.ServiceAccounts?.Any() == true)
        {
            async Task<string?> GetServiceAccountSecret(string technicalClientId)
            {
                var internalClientId = await _provisioningManager.GetIdOfCentralClientAsync(technicalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
                var authData = await _provisioningManager.GetCentralClientAuthDataAsync(internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
                return authData.Secret;
            }

            var tasks = data.ServiceAccounts.Select(async serviceAccount =>
                        {
                            if (serviceAccount.TechnicalClientId == null)
                            {
                                throw new ConflictException($"ClientId of serviceAccount {serviceAccount.TechnicalUserId} should be set");
                            }

                            var secret = serviceAccount.TechnicalUserKindId == TechnicalUserKindId.INTERNAL
                                ? await GetServiceAccountSecret(serviceAccount.TechnicalClientId).ConfigureAwait(ConfigureAwaitOptions.None)
                                : null;

                            return new CallbackTechnicalUserInfoData(
                                serviceAccount.TechnicalUserId,
                                secret,
                                serviceAccount.TechnicalClientId);
                        });
            technicalUsersInfoData = await Task.WhenAll(tasks).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        var callbackData = new OfferProviderCallbackData(
            technicalUsersInfoData,
            new CallbackClientInfoData(data.ClientId)
        );
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(data.AuthDetails!.EncryptionMode);
        var secret = cryptoHelper.Decrypt(
            data.AuthDetails!.ClientSecret,
            data.AuthDetails.InitializationVector);

        await _offerProviderService
            .TriggerOfferProviderCallback(callbackData, data.CallbackUrl, data.AuthDetails.AuthUrl, data.AuthDetails.ClientId, secret, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return (
            null,
            ProcessStepStatusId.DONE,
            true,
            null);
    }
}

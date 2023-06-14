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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Text.Json;

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
        var data = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetTriggerProviderInformation(offerSubscriptionId).ConfigureAwait(false);
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
            var autoSetupData = new OfferThirdPartyAutoSetupData(
                new OfferThirdPartyAutoSetupCustomerData(
                    data.CompanyInformationData.OrganizationName,
                    data.CompanyInformationData.Country,
                    data.UserEmail),
                new OfferThirdPartyAutoSetupPropertyData(
                    data.CompanyInformationData.BusinessPartnerNumber,
                    offerSubscriptionId,
                    data.OfferId)
            );
            await _offerProviderService
                .TriggerOfferProvider(autoSetupData, data.AutoSetupUrl!, cancellationToken)
                .ConfigureAwait(false);
        }

        var content = JsonSerializer.Serialize(new
        {
            AppName = data.OfferName,
            data.OfferId,
            RequestorCompanyName = data.CompanyInformationData.OrganizationName,
            data.UserEmail,
            AutoSetupExecuted = triggerProvider
        });
        await SendNotifications(data.OfferId, data.OfferTypeId, data.SalesManagerId, data.CompanyUserId, content).ConfigureAwait(false);
        return (
            new[] {
                data.IsSingleInstance ?
                    ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION :
                    ProcessStepTypeId.START_AUTOSETUP },
            triggerProvider ? ProcessStepStatusId.DONE : ProcessStepStatusId.SKIPPED,
            true,
            null);
    }

    private async Task SendNotifications(
        Guid offerId,
        OfferTypeId offerTypeId,
        Guid? salesManagerId,
        Guid companyUserId,
        string notificationContent)
    {
        var serviceManagerRoles = _settings.ServiceManagerRoles;
        var notificationRepository = _portalRepositories.GetInstance<INotificationRepository>();

        var notificationTypeId = offerTypeId == OfferTypeId.SERVICE ? NotificationTypeId.SERVICE_REQUEST : NotificationTypeId.APP_SUBSCRIPTION_REQUEST;
        if (salesManagerId.HasValue)
        {
            notificationRepository.CreateNotification(salesManagerId.Value, notificationTypeId, false,
                notification =>
                {
                    notification.CreatorUserId = companyUserId;
                    notification.Content = notificationContent;
                });
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(serviceManagerRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < serviceManagerRoles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", serviceManagerRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        await foreach (var receiver in _portalRepositories.GetInstance<IUserRepository>().GetServiceProviderCompanyUserWithRoleIdAsync(offerId, roleData))
        {
            if (salesManagerId.HasValue && receiver == salesManagerId.Value)
            {
                continue;
            }

            notificationRepository.CreateNotification(
                receiver,
                notificationTypeId,
                false,
                notification =>
                {
                    notification.CreatorUserId = companyUserId;
                    notification.Content = notificationContent;
                });
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerProviderCallback(Guid offerSubscriptionId, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetTriggerProviderCallbackInformation(offerSubscriptionId).ConfigureAwait(false);
        if (data == default)
        {
            throw new NotFoundException($"OfferSubscription {offerSubscriptionId} does not exist");
        }

        if (data.Status != OfferSubscriptionStatusId.ACTIVE)
        {
            throw new ConflictException("offer subscription should be active");
        }

        if (string.IsNullOrWhiteSpace(data.ClientId))
        {
            throw new ConflictException("Client should be set");
        }

        if (string.IsNullOrWhiteSpace(data.CallbackUrl))
        {
            throw new ConflictException("Callback Url should be set here");
        }

        if (data.ServiceAccounts.Count() > 1)
        {
            throw new ConflictException("There should be not more than one service account for the offer subscription");
        }

        CallbackTechnicalUserInfoData? technicalUserInfoData = null;
        if (data.ServiceAccounts.Count() == 1)
        {
            var serviceAccount = data.ServiceAccounts.FirstOrDefault();
            if (serviceAccount != default && serviceAccount.TechnicalClientId == null)
            {
                throw new ConflictException($"ClientId of serviceAccount {serviceAccount.TechnicalUserId} should be set");
            }
            var authData = await _provisioningManager.GetCentralClientAuthDataAsync(serviceAccount.TechnicalClientId).ConfigureAwait(false);
            technicalUserInfoData = new CallbackTechnicalUserInfoData(
                serviceAccount.TechnicalUserId,
                authData.Secret,
                serviceAccount.TechnicalClientId);
        }

        var callbackData = new OfferProviderCallbackData(
            technicalUserInfoData,
            new CallbackClientInfoData(data.ClientId)
        );
        await _offerProviderService
            .TriggerOfferProviderCallback(callbackData, data.CallbackUrl!, cancellationToken)
            .ConfigureAwait(false);

        return (
            null,
            ProcessStepStatusId.DONE,
            true,
            null);
    }
}

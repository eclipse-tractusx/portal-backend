/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Microsoft.Extensions.Logging;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;
using PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

public class OfferSubscriptionService : IOfferSubscriptionService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSetupService _offerSetupService;
    private readonly ILogger<OfferSubscriptionService> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerSetupService">SetupService for the 3rd Party Service Provider</param>
    /// <param name="logger">Access to the logger</param>
    public OfferSubscriptionService(
        IPortalRepositories portalRepositories, 
        IOfferSetupService offerSetupService, 
        ILogger<OfferSubscriptionService> logger)
    {
        _portalRepositories = portalRepositories;
        _offerSetupService = offerSetupService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> AddOfferSubscriptionAsync(Guid offerId, string iamUserId, string accessToken, IDictionary<string, IEnumerable<string>> serviceManagerRoles, OfferTypeId offerTypeId)
    {
        var offerProviderDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferProviderDetailsAsync(offerId, offerTypeId).ConfigureAwait(false);
        if (offerProviderDetails == null)
        {
            throw new NotFoundException($"Service {offerId} does not exist");
        }

        var (companyInformation, companyUserId, userEmail) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(iamUserId).ConfigureAwait(false);
        if (companyInformation.CompanyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company assigned", nameof(iamUserId));
        }
        
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company user assigned", nameof(iamUserId));
        }

        if (companyInformation.BusinessPartnerNumber == null)
        {
            throw new ConflictException($"company {companyInformation.OrganizationName} has no BusinessPartnerNumber assigned");
        }

        var offerSubscription = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);

        var autoSetupResult = string.Empty;
        if (!string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl))
        {
            try
            {
                var autoSetupData = new OfferThirdPartyAutoSetupData(
                    new OfferThirdPartyAutoSetupCustomerData(
                        companyInformation.OrganizationName,
                        companyInformation.Country,
                        userEmail),
                    new OfferThirdPartyAutoSetupPropertyData(
                        companyInformation.BusinessPartnerNumber,
                        offerSubscription.Id,
                        offerId)
                );
                await _offerSetupService.AutoSetupOffer(autoSetupData, iamUserId, accessToken, offerProviderDetails.AutoSetupUrl).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error occure while executing AutoSetupOffer: {ErrorMessage}", e.Message);
                autoSetupResult = e.Message;
            }
        }

        await SendNotifications(offerId, offerTypeId, offerProviderDetails, companyInformation, userEmail, autoSetupResult, companyUserId, serviceManagerRoles).ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return offerSubscription.Id;
    }

    private async Task SendNotifications(
        Guid offerId,
        OfferTypeId offerTypeId,
        OfferProviderDetailsData offerProviderDetails,
        CompanyInformationData companyInformation,
        string? userEmail,
        string autoSetupResult,
        Guid companyUserId,
        IDictionary<string, IEnumerable<string>> serviceManagerRoles)
    {
        var notificationRepository = _portalRepositories.GetInstance<INotificationRepository>();
        var notificationContent = JsonSerializer.Serialize(new
        {
            offerProviderDetails.AppName,
            offerId,
            RequestorCompanyName = companyInformation.OrganizationName,
            UserEmail = userEmail,
            AutoSetupExecuted = !string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl),
            AutoSetupError = autoSetupResult
        });
        var notificationTypeId = GetOfferSubscriptionNotificationType(offerTypeId);

        if (offerProviderDetails.SalesManagerId.HasValue)
        {
            notificationRepository.CreateNotification(offerProviderDetails.SalesManagerId.Value, notificationTypeId, false,
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
        if (roleData.Count < serviceManagerRoles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", serviceManagerRoles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{string.Join(", ", clientRoles.Value)}]"))}");
        }
        
        await foreach (var receiver in _portalRepositories.GetInstance<IUserRepository>().GetServiceProviderCompanyUserWithRoleIdAsync(offerId, roleData))
        {
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

    private static NotificationTypeId GetOfferSubscriptionNotificationType(OfferTypeId offerTypeId)
    {
        var appSubscriptionRequest = offerTypeId == OfferTypeId.SERVICE ? NotificationTypeId.SERVICE_REQUEST : NotificationTypeId.APP_SUBSCRIPTION_REQUEST;
        return appSubscriptionRequest;
    }
}

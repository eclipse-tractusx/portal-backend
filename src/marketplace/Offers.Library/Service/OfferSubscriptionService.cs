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

using System.Text.Json;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

public class OfferSubscriptionService : IOfferSubscriptionService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSetupService _offerSetupService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerSetupService">SetupService for the 3rd Party Service Provider</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="settings">Access to the settings</param>
    public OfferSubscriptionService(
        IPortalRepositories portalRepositories, 
        IOfferSetupService offerSetupService, 
        IOfferService offerService)
    {
        _portalRepositories = portalRepositories;
        _offerSetupService = offerSetupService;
    }

    /// <inheritdoc />
    public async Task<Guid> AddServiceSubscription(Guid serviceId, string iamUserId, string accessToken, OfferTypeId offerTypeId)
    {
        var serviceDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferProviderDetailsAsync(serviceId, offerTypeId).ConfigureAwait(false);
        if (serviceDetails == null)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        var (companyId, companyUserId, companyName, requesterEmail) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company assigned", nameof(iamUserId));
        }
        
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company user assigned", nameof(iamUserId));
        }

        var offerSubscription = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateOfferSubscription(serviceId, companyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);
        var autoSetupResult = string.Empty;
        if (!string.IsNullOrWhiteSpace(serviceDetails.AutoSetupUrl))
        {
            try
            {
                await _offerSetupService.AutoSetupOffer(offerSubscription.Id, iamUserId, accessToken, serviceDetails.AutoSetupUrl).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                autoSetupResult = e.Message;
            }
        }

        if (serviceDetails.SalesManagerId.HasValue)
        {
            var notificationContent = new
            {
                serviceDetails.AppName,
                RequestorCompanyName = companyName,
                UserEmail = requesterEmail,
                AutoSetupExecuted = !string.IsNullOrWhiteSpace(serviceDetails.AutoSetupUrl),
                AutoSetupError = autoSetupResult
            };
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(serviceDetails.SalesManagerId.Value, NotificationTypeId.APP_SUBSCRIPTION_REQUEST, false,
                notification =>
                {
                    notification.CreatorUserId = companyUserId;
                    notification.Content = JsonSerializer.Serialize(notificationContent);
                });
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return offerSubscription.Id;
    }
}

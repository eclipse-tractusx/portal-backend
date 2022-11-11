/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

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
    public async Task<Guid> AddServiceSubscription(Guid serviceId, string iamUserId, string accessToken, OfferTypeId offerTypeId)
    {
        var serviceDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferProviderDetailsAsync(serviceId, offerTypeId).ConfigureAwait(false);
        if (serviceDetails == null)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
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

        var offerSubscription = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateOfferSubscription(serviceId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);

        var autoSetupResult = string.Empty;
        if (!string.IsNullOrWhiteSpace(serviceDetails.AutoSetupUrl))
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
                        serviceId)
                );
                await _offerSetupService.AutoSetupOffer(autoSetupData, iamUserId, accessToken, serviceDetails.AutoSetupUrl).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error occure while executing AutoSetupOffer: {ErrorMessage}", e.Message);
                autoSetupResult = e.Message;
            }
        }

        if (serviceDetails.SalesManagerId.HasValue)
        {
            var notificationContent = new
            {
                serviceDetails.AppName,
                RequestorCompanyName = companyInformation.OrganizationName,
                UserEmail = userEmail,
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

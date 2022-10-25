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
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

public class OfferSubscriptionService : IOfferSubscriptionService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IMailingService _mailingService;
    private readonly ILogger<OfferSubscriptionService> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerSetupService">SetupService for the 3rd Party Service Provider</param>
    /// <param name="mailingService">Mail service.</param>
    /// <param name="logger">Access to the logger</param>
    public OfferSubscriptionService(
        IPortalRepositories portalRepositories, 
        IOfferSetupService offerSetupService,
        IMailingService mailingService,
        ILogger<OfferSubscriptionService> logger)
    {
        _portalRepositories = portalRepositories;
        _offerSetupService = offerSetupService;
        _mailingService = mailingService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> AddOfferSubscriptionAsync(Guid offerId, string iamUserId, string accessToken, IDictionary<string, IEnumerable<string>> serviceManagerRoles, OfferTypeId offerTypeId, string basePortalAddress)
    {
        var offerProviderDetails = await ValidateOfferProviderDetailDataAsync(offerId, offerTypeId).ConfigureAwait(false);
        var (companyInformation, companyUserId, userEmail) = await ValidateCompanyInformationAsync(iamUserId).ConfigureAwait(false);

        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerSubscription = offerTypeId == OfferTypeId.APP
            ? await HandleAppSubscriptionAsync(offerId, offerSubscriptionsRepository, companyInformation, companyUserId).ConfigureAwait(false)
            : offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);

        var autoSetupResult = await ExecuteAutoSetupAsync(offerId, iamUserId, accessToken, offerProviderDetails, companyInformation, userEmail, offerSubscription).ConfigureAwait(false);
        var notificationContent = JsonSerializer.Serialize(new
        {
            AppName = offerProviderDetails.OfferName,
            offerId,
            RequestorCompanyName = companyInformation.OrganizationName,
            UserEmail = userEmail,
            AutoSetupExecuted = !string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl),
            AutoSetupError = autoSetupResult ?? string.Empty
        });
        await SendNotifications(offerId, offerTypeId, offerProviderDetails, companyUserId, notificationContent, serviceManagerRoles).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(offerProviderDetails.ProviderContactEmail)) 
            return offerSubscription.Id;
        
        var mailParams = new Dictionary<string, string>
        {
            { "offerProviderName", offerProviderDetails.ProviderName},
            { "offerName", offerProviderDetails.OfferName! },
            { "url", basePortalAddress },
        };
        await _mailingService.SendMails(offerProviderDetails.ProviderContactEmail!, mailParams, new List<string> { "subscription-request" }).ConfigureAwait(false);
        return offerSubscription.Id;
    }

    private async Task<OfferProviderDetailsData> ValidateOfferProviderDetailDataAsync(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerProviderDetails = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetOfferProviderDetailsAsync(offerId, offerTypeId).ConfigureAwait(false);
        if (offerProviderDetails == null)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }

        if (offerProviderDetails.OfferName is not null)
            return offerProviderDetails;

        throw new ConflictException($"The offer name has not been configured properly");
    }

    private async Task<(CompanyInformationData companyInformation, Guid companyUserId, string? userEmail)> ValidateCompanyInformationAsync(string iamUserId)
    {
        var (companyInformation, companyUserId, userEmail) = await _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(iamUserId).ConfigureAwait(false);
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
            throw new ConflictException(
                $"company {companyInformation.OrganizationName} has no BusinessPartnerNumber assigned");
        }

        return (companyInformation, companyUserId, userEmail);
    }

    private static async Task<OfferSubscription> HandleAppSubscriptionAsync(
        Guid offerId,
        IOfferSubscriptionsRepository offerSubscriptionsRepository,
        CompanyInformationData companyInformation,
        Guid companyUserId)
    {
        OfferSubscription offerSubscription;
        var (offerSubscriptionId, offerSubscriptionStateId) = await offerSubscriptionsRepository
            .GetOfferSubscriptionStateForCompanyAsync(offerId, companyInformation.CompanyId, OfferTypeId.APP)
            .ConfigureAwait(false);
        if (offerSubscriptionStateId == default)
        {
            offerSubscription = offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId,
                OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);
        }
        else
        {
            if (offerSubscriptionStateId is OfferSubscriptionStatusId.ACTIVE or OfferSubscriptionStatusId.PENDING)
            {
                throw new ConflictException(
                    $"company {companyInformation.CompanyId} is already subscribed to {offerId}");
            }

            offerSubscription = offerSubscriptionsRepository.AttachAndModifyOfferSubscription(offerSubscriptionId,
                os => {
                    os.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
                    os.LastEditorId = companyUserId;
                });
        }

        return offerSubscription;
    }

    private async Task<string?> ExecuteAutoSetupAsync(
        Guid offerId, 
        string iamUserId, 
        string accessToken,
        OfferProviderDetailsData offerProviderDetails, 
        CompanyInformationData companyInformation, 
        string? userEmail,
        OfferSubscription offerSubscription)
    {
        if (string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl)) return null;
        
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
            await _offerSetupService
                .AutoSetupOffer(autoSetupData, iamUserId, accessToken, offerProviderDetails.AutoSetupUrl)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Error occure while executing AutoSetupOffer: {ErrorMessage}", e.Message);
            return e.Message;
        }

        return null;
    }

    private async Task SendNotifications(
        Guid offerId,
        OfferTypeId offerTypeId,
        OfferProviderDetailsData offerProviderDetails,
        Guid companyUserId,
        string notificationContent,
        IDictionary<string, IEnumerable<string>> serviceManagerRoles)
    {
        var notificationRepository = _portalRepositories.GetInstance<INotificationRepository>();
        
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

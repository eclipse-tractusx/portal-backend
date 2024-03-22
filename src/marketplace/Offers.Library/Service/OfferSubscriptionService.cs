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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using System.Collections.Immutable;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferSubscriptionService : IOfferSubscriptionService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;
    private readonly IMailingProcessCreation _mailingProcessCreation;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="identityService">Access to the identity of the user</param>
    /// <param name="mailingProcessCreation">Mail service.</param>
    public OfferSubscriptionService(
        IPortalRepositories portalRepositories,
        IIdentityService identityService,
        IMailingProcessCreation mailingProcessCreation)
    {
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _mailingProcessCreation = mailingProcessCreation;
    }

    /// <inheritdoc />
    public async Task<Guid> AddOfferSubscriptionAsync(Guid offerId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, OfferTypeId offerTypeId, string basePortalAddress, IEnumerable<UserRoleConfig> notificationRecipients, IEnumerable<UserRoleConfig> serviceManagerRoles)
    {
        var companyInformation = await ValidateCompanyInformationAsync(_identityData.CompanyId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        var offerProviderDetails = await ValidateOfferProviderDetailDataAsync(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (offerProviderDetails.ProviderCompanyId == null)
        {
            throw new ConflictException($"{offerTypeId} providing company is not set");
        }

        var activeAgreementConsents = await ValidateConsent(offerAgreementConsentData, offerId).ConfigureAwait(ConfigureAwaitOptions.None);

        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerSubscription = offerTypeId == OfferTypeId.APP
            ? await HandleAppSubscriptionAsync(offerId, offerSubscriptionsRepository, companyInformation, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None)
            : offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, _identityData.IdentityId);

        CreateProcessSteps(offerSubscription);
        CreateConsentsForSubscription(offerSubscription.Id, activeAgreementConsents, companyInformation.CompanyId, _identityData.IdentityId);

        var content = JsonSerializer.Serialize(new
        {
            AppName = offerProviderDetails.OfferName,
            OfferId = offerId,
            RequesterCompanyName = companyInformation.OrganizationName,
            UserEmail = companyInformation.CompanyUserEmail,
            AutoSetupExecuted = !string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl) && !offerProviderDetails.IsSingleInstance
        });
        await SendNotifications(offerId, offerTypeId, offerProviderDetails.SalesManagerId, _identityData.IdentityId, content, serviceManagerRoles).ConfigureAwait(ConfigureAwaitOptions.None);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        await _mailingProcessCreation.RoleBaseSendMail(
            notificationRecipients,
            new[]
            {
                ("offerName", offerProviderDetails.OfferName!),
                ("url", basePortalAddress)
            },
            ("offerProviderName", "User"),
            new[]
            {
                $"{offerTypeId.ToString().ToLower()}-subscription-request"
            },
            offerProviderDetails.ProviderCompanyId.Value).ConfigureAwait(ConfigureAwaitOptions.None);

        return offerSubscription.Id;
    }

    private void CreateProcessSteps(OfferSubscription offerSubscription)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var process = processStepRepository.CreateProcess(ProcessTypeId.OFFER_SUBSCRIPTION);
        offerSubscription.ProcessId = process.Id;
        processStepRepository.CreateProcessStepRange(new (ProcessStepTypeId, ProcessStepStatusId, Guid)[] { (ProcessStepTypeId.TRIGGER_PROVIDER, ProcessStepStatusId.TODO, process.Id) });
    }

    private async Task SendNotifications(
        Guid offerId,
        OfferTypeId offerTypeId,
        Guid? salesManagerId,
        Guid companyUserId,
        string notificationContent,
        IEnumerable<UserRoleConfig> serviceManagerRoles)
    {
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

    private async Task<OfferProviderDetailsData> ValidateOfferProviderDetailDataAsync(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerProviderDetails = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetOfferProviderDetailsAsync(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerProviderDetails == null)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }

        if (offerProviderDetails.OfferName is not null)
            return offerProviderDetails;

        throw new ConflictException("The offer name has not been configured properly");
    }

    private async Task<IEnumerable<OfferAgreementConsentData>> ValidateConsent(IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, Guid offerId)
    {
        var agreementData = await _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementIdsForOfferAsync(offerId).ToListAsync().ConfigureAwait(false);

        offerAgreementConsentData
            .ExceptBy(
                agreementData.Select(x => x.AgreementId),
                x => x.AgreementId)
            .IfAny(invalidConsents =>
                throw new ControllerArgumentException($"agreements {string.Join(",", invalidConsents.Select(consent => consent.AgreementId))} are not valid for offer {offerId}", nameof(offerAgreementConsentData)));

        agreementData.Where(x => x.AgreementStatusId == AgreementStatusId.ACTIVE)
            .ExceptBy(
                offerAgreementConsentData.Where(data => data.ConsentStatusId == ConsentStatusId.ACTIVE).Select(data => data.AgreementId),
                x => x.AgreementId)
            .IfAny(missing =>
                throw new ControllerArgumentException($"consent to agreements {string.Join(",", missing.Select(x => x.AgreementId))} must be given for offer {offerId}", nameof(offerAgreementConsentData)));

        // ignore consents for inactive agreements
        return offerAgreementConsentData
            .ExceptBy(
                agreementData.Where(x => x.AgreementStatusId == AgreementStatusId.INACTIVE).Select(x => x.AgreementId),
                consent => consent.AgreementId);
    }

    private async Task<CompanyInformationData> ValidateCompanyInformationAsync(Guid companyId, Guid companyUserId)
    {
        var companyInformation = await _portalRepositories.GetInstance<ICompanyRepository>()
            .GetOwnCompanyInformationAsync(companyId, companyUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (companyInformation == null)
        {
            throw new ControllerArgumentException($"Company {companyId} does not exist", nameof(companyId));
        }

        if (companyInformation.BusinessPartnerNumber == null)
        {
            throw new ConflictException($"company {companyInformation.OrganizationName} has no BusinessPartnerNumber assigned");
        }

        return companyInformation;
    }

    private static async Task<OfferSubscription> HandleAppSubscriptionAsync(
        Guid offerId,
        IOfferSubscriptionsRepository offerSubscriptionsRepository,
        CompanyInformationData companyInformation,
        Guid userId)
    {
        var activeOrPendingSubscriptionExists = await offerSubscriptionsRepository
            .CheckPendingOrActiveSubscriptionExists(offerId, companyInformation.CompanyId, OfferTypeId.APP)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (activeOrPendingSubscriptionExists)
        {
            throw new ConflictException($"company {companyInformation.CompanyId} is already subscribed to {offerId}");
        }

        return offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, userId);
    }

    private void CreateConsentsForSubscription(Guid offerSubscriptionId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, Guid companyId, Guid companyUserId)
    {
        foreach (var consentData in offerAgreementConsentData)
        {
            var consent = _portalRepositories.GetInstance<IConsentRepository>()
                .CreateConsent(consentData.AgreementId, companyId, companyUserId, consentData.ConsentStatusId);
            _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()
                .CreateConsentAssignedOfferSubscription(consent.Id, offerSubscriptionId);
        }
    }

    private static readonly IEnumerable<OfferSubscriptionStatusId> _offerSubcriptionStatusIdFilterActive = ImmutableArray.Create(OfferSubscriptionStatusId.ACTIVE);
    private static readonly IEnumerable<OfferSubscriptionStatusId> _offerSubcriptionStatusIdFilterInActive = ImmutableArray.Create(OfferSubscriptionStatusId.INACTIVE);
    private static readonly IEnumerable<OfferSubscriptionStatusId> _offerSubcriptionStatusIdFilterPending = ImmutableArray.Create(OfferSubscriptionStatusId.PENDING);
    private static readonly IEnumerable<OfferSubscriptionStatusId> _offerSubcriptionStatusIdFilterDefault = ImmutableArray.Create(OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE, OfferSubscriptionStatusId.INACTIVE);

    public static IEnumerable<OfferSubscriptionStatusId> GetOfferSubscriptionFilterStatusIds(OfferSubscriptionStatusId? offerStatusIdFilter) =>
        offerStatusIdFilter switch
        {
            OfferSubscriptionStatusId.ACTIVE => _offerSubcriptionStatusIdFilterActive,
            OfferSubscriptionStatusId.INACTIVE => _offerSubcriptionStatusIdFilterInActive,
            OfferSubscriptionStatusId.PENDING => _offerSubcriptionStatusIdFilterPending,
            _ => _offerSubcriptionStatusIdFilterDefault
        };
}

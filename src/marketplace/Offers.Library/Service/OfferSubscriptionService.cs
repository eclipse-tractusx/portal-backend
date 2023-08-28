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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferSubscriptionService : IOfferSubscriptionService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="mailingService">Mail service.</param>
    public OfferSubscriptionService(
        IPortalRepositories portalRepositories,
        IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
    }

    /// <inheritdoc />
    public async Task<Guid> AddOfferSubscriptionAsync(Guid offerId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, (Guid UserId, Guid CompanyId) identity, OfferTypeId offerTypeId, string basePortalAddress, IEnumerable<UserRoleConfig> notificationRecipients)
    {
        var companyInformation = await ValidateCompanyInformationAsync(identity.CompanyId).ConfigureAwait(false);
        var offerProviderDetails = await ValidateOfferProviderDetailDataAsync(offerId, offerTypeId).ConfigureAwait(false);

        if (offerProviderDetails.ProviderCompanyId == null)
        {
            throw new ConflictException($"{offerTypeId} providing company is not set");
        }

        await ValidateConsent(offerAgreementConsentData, offerId).ConfigureAwait(false);

        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerSubscription = offerTypeId == OfferTypeId.APP
            ? await HandleAppSubscriptionAsync(offerId, offerSubscriptionsRepository, companyInformation, identity.UserId).ConfigureAwait(false)
            : offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, identity.UserId);

        CreateProcessSteps(offerSubscription);
        CreateConsentsForSubscription(offerSubscription.Id, offerAgreementConsentData, companyInformation.CompanyId, identity.UserId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        await SendMail(notificationRecipients, offerProviderDetails.OfferName!, basePortalAddress, offerProviderDetails.ProviderCompanyId.Value);
        return offerSubscription.Id;
    }

    private void CreateProcessSteps(OfferSubscription offerSubscription)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var process = processStepRepository.CreateProcess(ProcessTypeId.OFFER_SUBSCRIPTION);
        offerSubscription.ProcessId = process.Id;
        processStepRepository.CreateProcessStepRange(new (ProcessStepTypeId, ProcessStepStatusId, Guid)[] { (ProcessStepTypeId.TRIGGER_PROVIDER, ProcessStepStatusId.TODO, process.Id) });
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

        throw new ConflictException("The offer name has not been configured properly");
    }

    private async Task ValidateConsent(IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, Guid offerId)
    {
        var agreementIds = await _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementIdsForOfferAsync(offerId).ToListAsync().ConfigureAwait(false);

        var invalid = offerAgreementConsentData.Select(data => data.AgreementId).Except(agreementIds);
        if (invalid.Any())
        {
            throw new ControllerArgumentException($"agreements {string.Join(",", invalid)} are not valid for offer {offerId}", nameof(offerAgreementConsentData));
        }
        var missing = agreementIds.Except(offerAgreementConsentData.Where(data => data.ConsentStatusId == ConsentStatusId.ACTIVE).Select(data => data.AgreementId));
        if (missing.Any())
        {
            throw new ControllerArgumentException($"consent to agreements {string.Join(",", missing)} must be given for offer {offerId}", nameof(offerAgreementConsentData));
        }
    }

    private async Task<CompanyInformationData> ValidateCompanyInformationAsync(Guid companyId)
    {
        var companyInformation = await _portalRepositories.GetInstance<ICompanyRepository>()
            .GetOwnCompanyInformationAsync(companyId).ConfigureAwait(false);
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
            .ConfigureAwait(false);
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
    private async Task SendMail(IEnumerable<UserRoleConfig> receiverRoles, string offerName, string basePortalAddress, Guid companyId)
    {
        var receiverUserRoles = receiverRoles;
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(receiverUserRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < receiverUserRoles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException(
                $"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverUserRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserEmailForCompanyAndRoleId(roleData, companyId);
        await foreach (var (receiver, firstName, lastName) in companyUserWithRoleIdForCompany)
        {
            var userName = string.Join(" ", new[] { firstName, lastName }.Where(item => !string.IsNullOrWhiteSpace(item)));

            var mailParams = new Dictionary<string, string>
            {
                { "offerName", offerName },
                { "offerProviderName", !string.IsNullOrWhiteSpace(userName) ? userName : "User"},
                { "url", basePortalAddress }
            };
            await _mailingService.SendMails(receiver, mailParams, new List<string> { "subscription-request" }).ConfigureAwait(false);
        }
    }
}

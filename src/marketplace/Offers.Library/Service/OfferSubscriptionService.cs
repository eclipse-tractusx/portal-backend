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
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

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
    public async Task<Guid> AddOfferSubscriptionAsync(Guid offerId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, OfferTypeId offerTypeId, string basePortalAddress)
    {
        var (companyInformation, companyUserId) = await ValidateCompanyInformationAsync(iamUserId).ConfigureAwait(false);
        var offerProviderDetails = await ValidateOfferProviderDetailDataAsync(offerId, offerTypeId).ConfigureAwait(false);
        await ValidateConsent(offerAgreementConsentData, offerId).ConfigureAwait(false);

        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerSubscriptionId = offerTypeId == OfferTypeId.APP
            ? await HandleAppSubscriptionAsync(offerId, offerSubscriptionsRepository, companyInformation, identity.CompanyUserId).ConfigureAwait(false)
            : offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId, OfferSubscriptionStatusId.PENDING, identity.CompanyUserId, identity.CompanyUserId).Id;

        CreateConsentsForSubscription(offerSubscriptionId, offerAgreementConsentData, companyInformation.CompanyId, identity.CompanyUserId);

        var autoSetupResult = await AutoSetupOfferSubscription(offerId, accessToken, offerProviderDetails, companyInformation, userEmail, offerSubscriptionId, offerProviderDetails.IsSingleInstance).ConfigureAwait(false);
        var notificationContent = JsonSerializer.Serialize(new
        {
            AppName = offerProviderDetails.OfferName,
            OfferId = offerId,
            RequestorCompanyName = companyInformation.OrganizationName,
            UserEmail = userEmail,
            AutoSetupExecuted = !string.IsNullOrWhiteSpace(offerProviderDetails.AutoSetupUrl),
            AutoSetupError = autoSetupResult ?? string.Empty
        });
        await SendNotifications(offerId, offerTypeId, offerProviderDetails, identity.CompanyUserId, notificationContent, serviceManagerRoles).ConfigureAwait(false);
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

    private void CreateProcessSteps(OfferSubscription offerSubscription, Process? process, IEnumerable<ProcessStepTypeId>? processStepTypeIds)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        if (process == null)
        {
            process = processStepRepository.CreateProcess(ProcessTypeId.OFFER_SUBSCRIPTION);
            offerSubscription.ProcessId = process.Id;
        }
        if (processStepTypeIds == null || !processStepTypeIds.Any())
        {
            processStepRepository.CreateProcessStepRange(new (ProcessStepTypeId, ProcessStepStatusId, Guid)[] { (ProcessStepTypeId.TRIGGER_PROVIDER, ProcessStepStatusId.TODO, process.Id) });
        }
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

    private async Task<(CompanyInformationData companyInformation, Guid companyUserId)> ValidateCompanyInformationAsync(string iamUserId)
    {
        var (companyInformation, userEmail) = await _portalRepositories.GetInstance<IUserRepository>()
            .GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(identity.CompanyUserId).ConfigureAwait(false);
        if (companyInformation.CompanyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {identity.UserEntityId} has no company assigned", nameof(identity.UserEntityId));
        }

        if (companyInformation.BusinessPartnerNumber == null)
        {
            throw new ConflictException(
                $"company {companyInformation.OrganizationName} has no BusinessPartnerNumber assigned");
        }

        return (companyInformation, companyUserId);
    }

    private static async Task<(OfferSubscription, Process?, IEnumerable<ProcessStepTypeId>?)> HandleAppSubscriptionAsync(
        Guid offerId,
        IOfferSubscriptionsRepository offerSubscriptionsRepository,
        CompanyInformationData companyInformation,
        Guid companyUserId)
    {
        var result = await offerSubscriptionsRepository
            .GetOfferSubscriptionStateForCompanyAsync(offerId, companyInformation.CompanyId, OfferTypeId.APP)
            .ConfigureAwait(false);
        if (result == default)
        {
            return (offerSubscriptionsRepository.CreateOfferSubscription(offerId, companyInformation.CompanyId,
                OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId), null, null);
        }

        if (result.OfferSubscriptionStatusId is OfferSubscriptionStatusId.ACTIVE or OfferSubscriptionStatusId.PENDING)
        {
            throw new ConflictException(
                $"company {companyInformation.CompanyId} is already subscribed to {offerId}");
        }

        return (
            offerSubscriptionsRepository.AttachAndModifyOfferSubscription(result.OfferSubscriptionId,
                os =>
                {
                    os.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
                    os.LastEditorId = companyUserId;
                }),
            result.Process,
            result.ProcessStepTypeIds);
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
}

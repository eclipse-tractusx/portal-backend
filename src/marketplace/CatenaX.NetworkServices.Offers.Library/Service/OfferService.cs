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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Offers.Library.Service;

public class OfferService : IOfferService
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    public OfferService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, string iamUserId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(subscriptionId, iamUserId, offerTypeId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException("Company or CompanyUser not assigned correctly.", nameof(iamUserId));
        }

        var (companyId, offerSubscription, companyUserId) = result;
        if (offerSubscription is null)
        {
            throw new NotFoundException($"Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}");
        }

        if (!await _portalRepositories.GetInstance<IAgreementRepository>()
                .CheckAgreementExistsForSubscriptionAsync(agreementId, subscriptionId, offerTypeId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Invalid Agreement {agreementId} for subscription {subscriptionId}", nameof(agreementId));
        }

        var consent = _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(agreementId, companyId, companyUserId, consentStatusId, null);
        _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>().CreateConsentAssignedOfferSubscription(consent.Id, offerSubscription.Id);
        
        await _portalRepositories.SaveAsync();
        return consent.Id;
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, IEnumerable<ServiceAgreementConsentData> offerAgreementConsentDatas,
        string iamUserId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(subscriptionId, iamUserId, offerTypeId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException("Company or CompanyUser not assigned correctly.", nameof(iamUserId));
        }

        var (companyId, offerSubscription, companyUserId) = result;
        if (offerSubscription is null)
        {
            throw new NotFoundException($"Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}");
        }

        if (!await _portalRepositories
                .GetInstance<IAgreementRepository>()
                .CheckAgreementsExistsForSubscriptionAsync(offerAgreementConsentDatas.Select(x => x.AgreementId), subscriptionId, offerTypeId)
                .ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Invalid Agreements for subscription {subscriptionId}", nameof(offerAgreementConsentDatas));
        }

        var consentAssignedOfferSubscriptionRepository = _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>();
        var offerSubscriptionConsents = await consentAssignedOfferSubscriptionRepository
            .GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(subscriptionId, offerAgreementConsentDatas.Select(x => x.AgreementId))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var offerSubscriptionConsent in offerSubscriptionConsents)
        {
            var consent = new Consent(offerSubscriptionConsent.ConsentId)
                {
                    ConsentStatusId = offerSubscriptionConsent.ConsentStatusId
                };
            var dbConsent = _portalRepositories.Attach(consent);
            dbConsent.ConsentStatusId = offerAgreementConsentDatas.Single(x => x.AgreementId == offerSubscriptionConsent.AgreementId).ConsentStatusId;
        }
        
        foreach (var consentData in offerAgreementConsentDatas.ExceptBy(offerSubscriptionConsents.Select(x => x.AgreementId), consentData => consentData.AgreementId))
        {
            var consent = _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(consentData.AgreementId, companyId, companyUserId, consentData.ConsentStatusId, null);
            consentAssignedOfferSubscriptionRepository.CreateConsentAssignedOfferSubscription(consent.Id, offerSubscription.Id);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetOfferAgreement(Guid offerId, OfferTypeId offerTypeId) => 
        _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementDataForOfferId(offerId, offerTypeId);

    /// <inheritdoc />
    public async Task<ConsentDetailData> GetConsentDetailDataAsync(Guid consentId, OfferTypeId offerTypeId)
    {
        var consentDetails = await _portalRepositories.GetInstance<IConsentRepository>()
            .GetConsentDetailData(consentId, offerTypeId).ConfigureAwait(false);
        if (consentDetails is null)
        {
            throw new NotFoundException($"Consent {consentId} does not exist");
        }

        return consentDetails;
    }
}

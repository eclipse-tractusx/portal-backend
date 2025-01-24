/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AgreementRepository : IAgreementRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Creates a new instance of <see cref="AgreementRepository"/>
    /// </summary>
    /// <param name="context">Access to the database context</param>
    public AgreementRepository(PortalDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> CheckAgreementExistsForSubscriptionAsync(Guid agreementId, Guid subscriptionId, OfferTypeId offerTypeId) =>
        _context.Agreements.AnyAsync(agreement =>
            agreement.Id == agreementId &&
            agreement.AgreementAssignedOffers.Any(aao =>
                aao.Offer!.OfferTypeId == offerTypeId &&
                aao.Offer.OfferSubscriptions.Any(subscription => subscription.Id == subscriptionId)));

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetOfferAgreementDataForOfferId(Guid offerId, OfferTypeId offerTypeId) =>
        _context.Agreements
            .Where(x => x.AgreementStatusId == AgreementStatusId.ACTIVE && x.AgreementAssignedOffers.Any(offer => offer.Offer!.OfferTypeId == offerTypeId && offer.OfferId == offerId))
            .Select(x => new AgreementData(x.Id, x.Name, x.Mandatory))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<AgreementDocumentData> GetAgreementsForCompanyRolesUntrackedAsync() =>
        _context.Agreements
            .AsNoTracking()
            .Where(agreement => agreement.AgreementAssignedCompanyRoles.Any() && agreement.AgreementStatusId == AgreementStatusId.ACTIVE)
            .Select(agreement => new AgreementDocumentData(
                agreement.Id,
                agreement.Name,
                agreement.AgreementLink,
                agreement.DocumentId,
                agreement.Mandatory))
            .AsAsyncEnumerable();

    ///<inheritdoc/>
    public IAsyncEnumerable<AgreementDocumentData> GetAgreementDataForOfferType(OfferTypeId offerTypeId, string languageShortName) =>
        _context.Agreements
            .AsNoTracking()
            .Where(agreement => agreement.AgreementStatusId == AgreementStatusId.ACTIVE && agreement.AgreementAssignedOfferTypes.Any(aaot => aaot.OfferTypeId == offerTypeId))
            .Select(agreement => new AgreementDocumentData(
                agreement.Id,
                agreement.AgreementDescriptions.SingleOrDefault(x => x.LanguageShortName == languageShortName)!.Description,
                agreement.AgreementLink,
                agreement.DocumentId,
                agreement.Mandatory
            ))
            .AsAsyncEnumerable();

    ///<inheritdoc/>
    public Task<(OfferAgreementConsent OfferAgreementConsent, bool IsProviderCompany)> GetOfferAgreementConsentById(Guid offerId, Guid userCompanyId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .Where(offer => offer.Id == offerId &&
                offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<OfferAgreementConsent, bool>(
                new OfferAgreementConsent(
                    offer.ConsentAssignedOffers.Where(x => x.Consent!.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE)
                        .Select(consentAssignedOffer => new AgreementConsentStatus(
                            consentAssignedOffer.Consent!.AgreementId,
                            consentAssignedOffer.Consent.ConsentStatusId))),
                        offer.ProviderCompanyId == userCompanyId
            ))
            .SingleOrDefaultAsync();

    ///<inheritdoc/>
    public Task<(OfferAgreementConsentUpdate OfferAgreementConsentUpdate, bool IsProviderCompany)> GetOfferAgreementConsent(Guid appId, Guid companyId, OfferStatusId statusId, OfferTypeId offerTypeId) =>
        _context.Offers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(offer => offer.Id == appId &&
                offer.OfferStatusId == statusId &&
                offer.OfferTypeId == offerTypeId)
            .Select(offer => new ValueTuple<OfferAgreementConsentUpdate, bool>(
                new OfferAgreementConsentUpdate(
                    offer.ConsentAssignedOffers.Select(consentAssignedOffer => new AppAgreementConsentStatus(
                        consentAssignedOffer.Consent!.AgreementId,
                        consentAssignedOffer.Consent.Id,
                        consentAssignedOffer.Consent.ConsentStatusId)),
                    offer.OfferType!.AgreementAssignedOfferTypes.Select(assigned => new AgreementStatusData(assigned.AgreementId, assigned.Agreement!.AgreementStatusId))),
                offer.ProviderCompanyId == companyId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckAgreementsExistsForSubscriptionAsync(IEnumerable<Guid> agreementIds, Guid subscriptionId, OfferTypeId offerTypeId) =>
        _context.Agreements.AnyAsync(agreement =>
            agreementIds.Any(a => a == agreement.Id) &&
            agreement.AgreementAssignedOffers.Any(aao =>
                aao.Offer!.OfferTypeId == offerTypeId &&
                aao.Offer.OfferSubscriptions.Any(subscription => subscription.Id == subscriptionId)));

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementStatusData> GetAgreementIdsForOfferAsync(Guid offerId) =>
        _context.AgreementAssignedOffers
            .AsNoTracking()
            .Where(assigned => assigned.OfferId == offerId)
            .Select(assigned => new AgreementStatusData(assigned.AgreementId, assigned.Agreement!.AgreementStatusId))
            .AsAsyncEnumerable();

    public void AttachAndModifyAgreements(IEnumerable<(Guid AgreementId, Action<Agreement>? Initialize, Action<Agreement> Modify)> agreementModificationIds)
    {
        var items = agreementModificationIds.Select(agreementModificationId =>
        {
            var agreement = new Agreement(agreementModificationId.AgreementId, default, null!, default, default, default);
            agreementModificationId.Initialize?.Invoke(agreement);
            return (Agreement: agreement, agreementModificationId.Modify);
        }).ToList();
        _context.AttachRange(items.Select(item => item.Agreement));
        items.ForEach(item => item.Modify(item.Agreement));
    }
}

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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class ConsentRepository : IConsentRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates an instance of <see cref="ConsentRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The database</param>
    public ConsentRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc/>
    public Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields = null)
    {
        var consent = new Consent(Guid.NewGuid(), agreementId, companyId, companyUserId, consentStatusId, DateTimeOffset.UtcNow);
        setupOptionalFields?.Invoke(consent);
        return _portalDbContext.Consents.Add(consent).Entity;
    }

    /// <inheritdoc />
    public void RemoveConsents(IEnumerable<Consent> consents) => 
        _portalDbContext.RemoveRange(consents);

    ///<inheritdoc/>
    public ConsentAssignedOffer CreateConsentAssignedOffer(Guid consentId, Guid offerId) =>
        _portalDbContext.ConsentAssignedOffers.Add(new ConsentAssignedOffer(consentId, offerId)).Entity;

    /// <inheritdoc />
    public Task<ConsentDetailData?> GetConsentDetailData(Guid consentId, OfferTypeId offerTypeId) =>
        _portalDbContext.Consents
            .Where(consent =>
                consent.Id == consentId &&
                consent.Agreement!.AgreementAssignedOffers.Any(aao => aao.Offer!.OfferTypeId == offerTypeId))
            .Select(x => new ConsentDetailData(
                x.Id,
                x.Company!.Name,
                x.CompanyUserId,
                x.ConsentStatusId,
                x.Agreement!.Name
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifiesConsents(IEnumerable<Consent> consents, Action<Consent> setOptionalParameter)
    {
        _portalDbContext.Consents.AttachRange(consents);
        foreach (var consent in consents)
        {
            setOptionalParameter.Invoke(consent);
        }
    }
}

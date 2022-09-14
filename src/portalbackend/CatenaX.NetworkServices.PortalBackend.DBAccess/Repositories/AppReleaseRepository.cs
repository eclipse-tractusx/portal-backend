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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Implementation of <see cref="IAppReleaseRepository"/> accessing database with EF Core.
/// </summary>
public class AppReleaseRepository : IAppReleaseRepository
{
    private readonly PortalDbContext _context;
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext"></param>
    public AppReleaseRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }
    
    ///<inheritdoc/>
    public  Task<Guid> GetCompanyUserIdForAppUntrackedAsync(Guid appId, string userId)
    =>
        _context.Offers
            .Where(a => a.Id == appId && a.OfferStatusId == OfferStatusId.CREATED)
            .Select(x=>x.ProviderCompany!.CompanyUsers.First(companyUser => companyUser.IamUser!.UserEntityId == userId).Id)
            .SingleOrDefaultAsync();
    
    ///<inheritdoc/>
    public OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId) =>
        _context.OfferAssignedDocuments.Add(new OfferAssignedDocument(offerId, documentId)).Entity;
    
    ///<inheritdoc/>
    public Task<bool> IsProviderCompanyUserAsync(Guid appId, string userId) =>
        _context.Offers
            .AnyAsync(a => a.Id == appId
                && a.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId));

    ///<inheritdoc/>
    public UserRole CreateAppUserRole(Guid appId, string role) =>
        _context.UserRoles.Add(
            new UserRole(
                Guid.NewGuid(),
                role,
                appId
            ))
            .Entity;

    ///<inheritdoc/>
    public UserRoleDescription CreateAppUserRoleDescription(Guid roleId, string languageCode, string description) =>
        _context.UserRoleDescriptions.Add(
            new UserRoleDescription(
                roleId,
                languageCode,
                description
            ))
            .Entity;

    public IAsyncEnumerable<AgreementData> GetAgreements()
    =>
        _context.Agreements
            .AsNoTracking()
            .Where(agreement=>agreement.AgreementCategoryId == AgreementCategoryId.APP_CONTRACT)
            .Select(agreement=> new  AgreementData(
                agreement.Id,
                agreement.Name
            ))
            .AsAsyncEnumerable();
    

    public Task<OfferAgreementConsent?> GetOfferAgreementConsentById(Guid appId, string userId)
    =>
        _context.Offers
            .AsNoTracking()
            .Where(offer=>offer.Id == appId && offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId))
            .Select(offer=> new OfferAgreementConsent(
                offer.ConsentAssignedOffers!.Where(consentAssignedOffer => consentAssignedOffer.Consent!.Agreement!.AgreementCategoryId == AgreementCategoryId.APP_CONTRACT).Select(consentAssignedOffer => new AgreementConsentStatus(
                consentAssignedOffer.Consent!.AgreementId,
                consentAssignedOffer.Consent!.ConsentStatusId
            ))))
            .SingleOrDefaultAsync();
    
    public Task<OfferAgreementConsents?> GetOfferAgreementConsent(Guid appId, string userId)
    =>
        _context.Offers
            .AsNoTracking()
            .Where(offer=>offer.Id == appId && offer.OfferStatusId == OfferStatusId.CREATED 
            && offer.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == userId))
            .Select(offer=> new OfferAgreementConsents(
                offer.ProviderCompany!.CompanyUsers.Select(companyUser=>companyUser.Id).SingleOrDefault(),
                offer.ProviderCompany.Id,
                offer.ConsentAssignedOffers!.Where(consentAssignedOffer => consentAssignedOffer.Consent!.Agreement!.AgreementCategoryId == AgreementCategoryId.APP_CONTRACT).Select(consentAssignedOffer => new AppAgreementConsentStatus(
                consentAssignedOffer.Consent!.AgreementId,
                consentAssignedOffer.Consent!.Id,
                consentAssignedOffer.Consent!.ConsentStatusId
            ))))
            .SingleOrDefaultAsync();
}

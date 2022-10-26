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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IOfferSubscriptionsRepository"/> accessing database with EF Core.
public class OfferSubscriptionsRepository : IOfferSubscriptionsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public OfferSubscriptionsRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc />
    public OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId) =>
        _context.OfferSubscriptions.Add(new OfferSubscription(Guid.NewGuid(), offerId, companyId, offerSubscriptionStatusId, requesterId, creatorId)).Entity;

    /// <inheritdoc />
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(string iamUserId) =>
        _context.IamUsers.AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.OfferSubscriptions)
            .Select(s => new AppWithSubscriptionStatus(s.OfferId, s.OfferSubscriptionStatusId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetOwnCompanyProvidedAppSubscriptionStatusesUntrackedAsync(string iamUserId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(s => s.Offer!.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .GroupBy(s => s.OfferId)
            .Select(g => new AppCompanySubscriptionStatusData
            {
                AppId = g.Key,
                CompanySubscriptionStatuses = g.Select(s =>
                    new CompanySubscriptionStatusData(s.CompanyId, s.OfferSubscriptionStatusId))
            })
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(OfferSubscription? companyAssignedApp, bool isMemberOfCompanyProvidingApp, string? appName, Guid companyUserId)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId) =>
        _context.Offers
            .Where(app => app.Id == appId)
            .Select(app => new ValueTuple<OfferSubscription?, bool, string?, Guid>(
                app.OfferSubscriptions.SingleOrDefault(assignedApp => assignedApp.CompanyId == companyId),
                app.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId),
                app.Name,
                app.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId) ? app.ProviderCompany!.CompanyUsers.First(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).Id : Guid.Empty
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferSubscription? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId) =>
        _context.Offers
            .Where(app => app.Id == appId)
            .Select(app => new ValueTuple<OfferSubscription?,bool>(
                app!.OfferSubscriptions.SingleOrDefault(assignedApp => assignedApp.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)),
                true
            ))
            .SingleOrDefaultAsync();

    public Task<(Guid companyId, OfferSubscription? offerSubscription, string companyName, Guid companyUserId)> GetCompanyIdWithAssignedOfferForCompanyUserAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.IamUsers
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new ValueTuple<Guid, OfferSubscription?, string, Guid>(
                company!.Id,
                company.OfferSubscriptions.SingleOrDefault(os => os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId),
                company!.Name,
                company.CompanyUsers.First(x => x.IamUser!.UserEntityId == iamUserId).Id
            ))
            .SingleOrDefaultAsync();

    public Task<(Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId)> GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.IamUsers
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new ValueTuple<Guid, OfferSubscription?, Guid>(
                company!.Id,
                company.OfferSubscriptions.SingleOrDefault(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId),
                company.CompanyUsers.First(x => x.IamUser!.UserEntityId == iamUserId).Id
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<SubscriptionDetailData?> GetSubscriptionDetailDataForOwnUserAsync(Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId && os.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId))
            .Select(os => new SubscriptionDetailData(os.OfferId, os.Offer!.Name!, os.OfferSubscriptionStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionTransferData?> GetOfferDetailsAndCheckUser(Guid offerSubscriptionId, string iamUserId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId && x.Offer!.OfferTypeId == offerTypeId)
            .Select(x => new OfferSubscriptionTransferData(
                    x.OfferSubscriptionStatusId, 
                    x.Offer!.ProviderCompany!.CompanyUsers.Where(cu => cu.IamUser!.UserEntityId == iamUserId).Select(cu => cu.Id).SingleOrDefault(),
                    x.Offer!.ProviderCompany!.CompanyServiceAccounts.Where(cu => cu.IamServiceAccount!.UserEntityId == iamUserId).Select(cu => cu.Id).SingleOrDefault(),
                    x.Company!.Name,
                    x.CompanyId,
                    x.RequesterId,
                    x.OfferId,
                    x.Offer!.Name!,
                    x.Company.BusinessPartnerNumber!,
                    x.Requester!.Email
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid offerSubscriptionId, OfferSubscriptionStatusId offerSubscriptionStatusId)> GetOfferSubscriptionStateForCompanyAsync(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(x => x.OfferId == offerId && x.CompanyId == companyId && x.Offer!.OfferTypeId == offerTypeId)
            .Select(x => new ValueTuple<Guid, OfferSubscriptionStatusId>(x.Id, x.OfferSubscriptionStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription>? setOptionalParameters = null)
    {
        var offerSubscription = _context.Attach(new OfferSubscription(offerSubscriptionId, Guid.Empty, Guid.Empty, default, Guid.Empty, Guid.Empty)).Entity;
        setOptionalParameters?.Invoke(offerSubscription);
        return offerSubscription;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<BusinessAppData> GetAllBusinessAppDataForUserIdAsync(string userId) =>
        _context.OfferSubscriptions.Where(x => 
                x.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == userId) &&
                x.Offer!.UserRoles.Any(ur => ur.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == userId)) &&
                x.AppSubscriptionDetail!.AppInstance != null
            )
            .Select(offerSubscription => new BusinessAppData(
                offerSubscription.Id,
                offerSubscription.Offer!.Name ?? Constants.ErrorString,
                offerSubscription.AppSubscriptionDetail!.AppSubscriptionUrl ?? Constants.ErrorString,
                offerSubscription.Offer!.ThumbnailUrl ?? Constants.ErrorString,
                offerSubscription.Offer!.Provider
            )).AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(OfferThirdPartyAutoSetupData AutoSetupData, bool IsUsersCompany)> GetThirdPartyAutoSetupDataAsync(Guid offerSubscriptionId, string iamUserId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new ValueTuple<OfferThirdPartyAutoSetupData,bool>(
                new OfferThirdPartyAutoSetupData(
                    new OfferThirdPartyAutoSetupCustomerData(
                        x.Company!.Name,
                        x.Company!.Address!.CountryAlpha2Code,
                        x.Company.CompanyUsers.Single(cu => cu.IamUser!.UserEntityId == iamUserId).Email),
                    new OfferThirdPartyAutoSetupPropertyData(
                        x.Company!.BusinessPartnerNumber,
                        offerSubscriptionId,
                        x.OfferId)),
                x.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();
}

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

using System.Linq.Expressions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
    public Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(string iamUserId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                _context.Offers
                    .AsNoTracking()
                    .Where(os => 
                        os.OfferTypeId == offerTypeId &&
                        os.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId) &&
                        (statusId == null || os.OfferSubscriptions.Any(x => x.OfferSubscriptionStatusId == statusId)))
                    .GroupBy(s => s.ProviderCompanyId),
                sorting switch
                {
                    SubscriptionStatusSorting.CompanyNameAsc => (IEnumerable<Offer> o) => o.OrderBy(offer => offer.ProviderCompany!.Name),
                    SubscriptionStatusSorting.CompanyNameDesc => (IEnumerable<Offer> o) => o.OrderByDescending(offer => offer.ProviderCompany!.Name),
                    SubscriptionStatusSorting.OfferIdAsc => (IEnumerable<Offer> o) => o.OrderBy(offer => offer.Id),
                    SubscriptionStatusSorting.OfferIdDesc => (IEnumerable<Offer> o) => o.OrderByDescending(offer => offer.Id),
                    _ => (Expression<Func<IEnumerable<Offer>,IOrderedEnumerable<Offer>>>?)null
                },
                g => new OfferCompanySubscriptionStatusData
                {
                    OfferId = g.Id,
                    ServiceName = g.Name,
                    CompanySubscriptionStatuses = g.OfferSubscriptions.Select(s =>
                        new CompanySubscriptionStatusData(s.CompanyId, s.Company!.Name, s.Id, s.OfferSubscriptionStatusId))
                })
            .SingleOrDefaultAsync();

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
                    x.Requester!.Email,
                    x.Requester!.Firstname
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid offerSubscriptionId, OfferSubscriptionStatusId offerSubscriptionStatusId)> GetOfferSubscriptionStateForCompanyAsync(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(x => x.OfferId == offerId && x.CompanyId == companyId && x.Offer!.OfferTypeId == offerTypeId)
            .Select(x => new ValueTuple<Guid, OfferSubscriptionStatusId>(x.Id, x.OfferSubscriptionStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription> setOptionalParameters)
    {
        var offerSubscription = _context.Attach(new OfferSubscription(offerSubscriptionId, Guid.Empty, Guid.Empty, default, Guid.Empty, Guid.Empty)).Entity;
        setOptionalParameters.Invoke(offerSubscription);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid SubscriptionId, string? OfferName, string SubscriptionUrl, string? ThumbnailUrl, string Provider)> GetAllBusinessAppDataForUserIdAsync(string iamUserId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(user => user.IamUser!.UserEntityId == iamUserId)
            .SelectMany(user => user.Company!.OfferSubscriptions.Where(subscription => 
                subscription.Offer!.UserRoles.Any(ur => ur.CompanyUsers.Any(cu => cu.Id == user.Id)) &&
                subscription.AppSubscriptionDetail!.AppInstance != null &&
                subscription.AppSubscriptionDetail.AppSubscriptionUrl != null))
            .Select(offerSubscription => new ValueTuple<Guid,string?,string,string?,string>(
                offerSubscription.Id,
                offerSubscription.Offer!.Name,
                offerSubscription.AppSubscriptionDetail!.AppSubscriptionUrl!,
                offerSubscription.Offer!.ThumbnailUrl,
                offerSubscription.Offer!.Provider
            )).ToAsyncEnumerable();
}

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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

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

    public IQueryable<CompanyUser> GetOwnCompanyAppUsersUntrackedAsync(
        Guid appId,
        string iamUserId,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? roleName = null) {

        char[] escapeChar = { '%', '_', '[', ']', '^' };
        return _context.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.UserRoles.Any(userRole => userRole.Offer!.Id == appId) && companyUser.IamUser!.UserEntityId == iamUserId)
            .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
            .Where(companyUser => firstName == null || EF.Functions.ILike(companyUser.Firstname!, $"%{firstName.Trim(escapeChar)}%")
                && lastName == null || EF.Functions.ILike(companyUser.Lastname!, $"%{lastName!.Trim(escapeChar)}%")
                && email == null || EF.Functions.ILike(companyUser.Email!, $"%{email!.Trim(escapeChar)}%")
                && roleName == null || companyUser.UserRoles.Any(userRole => EF.Functions.ILike(userRole.UserRoleText, $"{roleName!.Trim(escapeChar)}%")));
        }

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
                    x.Company.BusinessPartnerNumber!
            ))
            .SingleOrDefaultAsync();

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

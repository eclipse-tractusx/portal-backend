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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Linq.Expressions;

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
    public OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId) =>
        _context.OfferSubscriptions.Add(new OfferSubscription(Guid.NewGuid(), offerId, companyId, offerSubscriptionStatusId, requesterId)).Entity;

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, IEnumerable<OfferSubscriptionStatusId> statusIds, Guid? offerId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                _context.Offers
                    .AsNoTracking()
                    .Where(offer =>
                        offer.OfferTypeId == offerTypeId &&
                        (!offerId.HasValue || offer.Id == offerId.Value) &&
                        offer.ProviderCompanyId == userCompanyId &&
                        offer.OfferSubscriptions.Any(os => statusIds.Contains(os.OfferSubscriptionStatusId)))
                    .GroupBy(s => s.ProviderCompanyId),
                sorting switch
                {
                    SubscriptionStatusSorting.CompanyNameAsc => (IEnumerable<Offer> o) => o.OrderBy(offer => offer.ProviderCompany!.Name),
                    SubscriptionStatusSorting.CompanyNameDesc => (IEnumerable<Offer> o) => o.OrderByDescending(offer => offer.ProviderCompany!.Name),
                    SubscriptionStatusSorting.OfferIdAsc => (IEnumerable<Offer> o) => o.OrderBy(offer => offer.Id),
                    SubscriptionStatusSorting.OfferIdDesc => (IEnumerable<Offer> o) => o.OrderByDescending(offer => offer.Id),
                    _ => (Expression<Func<IEnumerable<Offer>, IOrderedEnumerable<Offer>>>?)null
                },
                g => new OfferCompanySubscriptionStatusData
                {
                    OfferId = g.Id,
                    ServiceName = g.Name,
                    CompanySubscriptionStatuses = g.OfferSubscriptions
                        .Where(os => statusIds.Contains(os.OfferSubscriptionStatusId))
                        .Select(s =>
                            new CompanySubscriptionStatusData(
                                s.CompanyId,
                                s.Company!.Name,
                                s.Id,
                                s.OfferSubscriptionStatusId,
                                s.Company.Address!.CountryAlpha2Code,
                                s.Company.BusinessPartnerNumber,
                                s.Requester!.Email,
                                s.Offer!.TechnicalUserProfiles.Any(tup => tup.TechnicalUserProfileAssignedUserRoles.Any()))),
                    Image = g.Documents
                        .Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId == DocumentStatusId.LOCKED)
                        .Select(document => document.Id)
                        .FirstOrDefault()
                })
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferSubscriptionStatusId SubscriptionStatusId, Guid RequestorId, Guid AppId, string? AppName, bool IsUserOfProvider, RequesterData Requester)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid subscriptionId, Guid userCompanyId) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId)
            .Select(x => new ValueTuple<OfferSubscriptionStatusId, Guid, Guid, string?, bool, RequesterData>(
                x.OfferSubscriptionStatusId,
                x.RequesterId,
                x.Offer!.Id,
                x.Offer.Name,
                x.Offer.ProviderCompanyId == userCompanyId,
                new RequesterData(x.Requester!.Email, x.Requester.Firstname, x.Requester.Lastname)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferSubscriptionStatusId OfferSubscriptionStatusId, bool IsSubscribingCompany, bool IsValidSubscriptionId)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid subscriptionId, Guid userCompanyId) =>
        _context.OfferSubscriptions
            .Where(os =>
                os.Id == subscriptionId
            )
            .Select(os => new ValueTuple<OfferSubscriptionStatusId, bool, bool>(
                os.OfferSubscriptionStatusId,
                os.CompanyId == userCompanyId,
                true
            ))
            .SingleOrDefaultAsync();

    public Task<(Guid companyId, OfferSubscription? offerSubscription)> GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(Guid subscriptionId, Guid userId, OfferTypeId offerTypeId) =>
        _context.CompanyUsers
            .Where(user => user.Id == userId)
            .Select(user => user.Identity!.Company)
            .Select(company => new ValueTuple<Guid, OfferSubscription?>(
                company!.Id,
                company.OfferSubscriptions.SingleOrDefault(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<SubscriptionDetailData?> GetSubscriptionDetailDataForOwnUserAsync(Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId && os.CompanyId == userCompanyId)
            .Select(os => new SubscriptionDetailData(os.OfferId, os.Offer!.Name!, os.OfferSubscriptionStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionTransferData?> GetOfferDetailsAndCheckProviderCompany(Guid offerSubscriptionId, Guid providerCompanyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId && x.Offer!.OfferTypeId == offerTypeId)
            .Select(x => new OfferSubscriptionTransferData(
                x.OfferSubscriptionStatusId,
                x.Offer!.ProviderCompanyId == providerCompanyId,
                x.Company!.Name,
                x.CompanyId,
                x.RequesterId,
                x.OfferId,
                x.Offer.OfferTypeId,
                x.Offer!.Name,
                x.Company.BusinessPartnerNumber,
                x.Requester!.Email,
                x.Requester.Firstname,
                x.Requester.Lastname,
                x.Offer.AppInstanceSetup == null ? new ValueTuple<bool, string?>() : new ValueTuple<bool, string?>(x.Offer.AppInstanceSetup.IsSingleInstance, x.Offer.AppInstanceSetup.InstanceUrl),
                x.Offer.AppInstances.Select(ai => ai.Id),
                x.Offer.SalesManagerId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckPendingOrActiveSubscriptionExists(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .AnyAsync(x =>
                x.OfferId == offerId &&
                x.CompanyId == companyId &&
                x.Offer!.OfferTypeId == offerTypeId &&
                (x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE || x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING));

    /// <inheritdoc />
    public OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription> setOptionalParameters)
    {
        var offerSubscription = _context.Attach(new OfferSubscription(offerSubscriptionId, Guid.Empty, Guid.Empty, default, Guid.Empty)).Entity;
        setOptionalParameters.Invoke(offerSubscription);
        return offerSubscription;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid OfferId, Guid SubscriptionId, string? OfferName, string SubscriptionUrl, Guid LeadPictureId, string Provider)> GetAllBusinessAppDataForUserIdAsync(Guid userId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Identity!.Company!.OfferSubscriptions.Where(subscription =>
                subscription.Offer!.UserRoles.Any(ur => ur.IdentityAssignedRoles.Any(cu => cu.IdentityId == user.Id && cu.Identity!.IdentityTypeId == IdentityTypeId.COMPANY_USER)) &&
                subscription.AppSubscriptionDetail!.AppInstance != null &&
                subscription.AppSubscriptionDetail.AppSubscriptionUrl != null))
            .Select(offerSubscription => new ValueTuple<Guid, Guid, string?, string, Guid, string>(
                offerSubscription.OfferId,
                offerSubscription.Id,
                offerSubscription.Offer!.Name,
                offerSubscription.AppSubscriptionDetail!.AppSubscriptionUrl!,
                offerSubscription.Offer!.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offerSubscription.Offer!.Provider
            )).ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool Exists, bool IsUserOfCompany, ProviderSubscriptionDetailData? Details)> GetSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds) =>
        _context.OfferSubscriptions
            .AsSplitQuery()
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId)
            .Select(os => new
            {
                IsProviderCompany = os.Offer!.ProviderCompanyId == userCompanyId,
                Subscription = os,
                Company = os.Company
            })
            .Select(x => new ValueTuple<bool, bool, ProviderSubscriptionDetailData?>(
                true,
                x.IsProviderCompany,
                x.IsProviderCompany
                    ? new ProviderSubscriptionDetailData(
                        x.Subscription.OfferId,
                        x.Subscription.OfferSubscriptionStatusId,
                        x.Subscription.Offer!.Name,
                        x.Company!.Name,
                        x.Company.BusinessPartnerNumber,
                        x.Company.Identities.Where(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!).Where(cu => cu.Email != null && cu.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Any(ur => userRoleIds.Contains(ur.Id))).Select(cu => cu.Email!),
                        x.Subscription.CompanyServiceAccounts.Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Select(ur => ur.UserRoleText))))
                    : null))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, bool IsUserOfCompany, AppProviderSubscriptionDetailData? Details)> GetAppSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds) =>
        _context.OfferSubscriptions
            .AsSplitQuery()
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId)
            .Select(os => new
            {
                IsProviderCompany = os.Offer!.ProviderCompanyId == userCompanyId,
                Subscription = os,
                Company = os.Company
            })
            .Select(x => new ValueTuple<bool, bool, AppProviderSubscriptionDetailData?>(
                true,
                x.IsProviderCompany,
                x.IsProviderCompany
                    ? new AppProviderSubscriptionDetailData(
                        x.Subscription.OfferId,
                        x.Subscription.OfferSubscriptionStatusId,
                        x.Subscription.Offer!.Name,
                        x.Company!.Name,
                        x.Company.BusinessPartnerNumber,
                        x.Company.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(id => id.CompanyUser!).Where(cu => cu.Email != null && cu.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Any(ur => userRoleIds.Contains(ur.Id))).Select(cu => cu.Email!),
                        x.Subscription.CompanyServiceAccounts.Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Select(ur => ur.UserRoleText))),
                        x.Subscription.AppSubscriptionDetail!.AppSubscriptionUrl,
                        x.Subscription.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId)
                    : null))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, bool IsUserOfCompany, SubscriberSubscriptionDetailData? Details)> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds) =>
        _context.OfferSubscriptions
            .AsSplitQuery()
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId)
            .Select(os => new
            {
                IsSubscriberCompany = os.CompanyId == userCompanyId,
                Subscription = os,
                ProviderCompany = os.Offer!.ProviderCompany
            })
            .Select(x => new ValueTuple<bool, bool, SubscriberSubscriptionDetailData?>(
                true,
                x.IsSubscriberCompany,
                x.IsSubscriberCompany
                    ? new SubscriberSubscriptionDetailData(
                        x.Subscription.OfferId,
                        x.Subscription.OfferSubscriptionStatusId,
                        x.Subscription.Offer!.Name,
                        x.ProviderCompany!.Name,
                        x.ProviderCompany.Identities.Where(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!).Where(cu => cu.Email != null && cu.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Any(ur => userRoleIds.Contains(ur.Id))).Select(cu => cu.Email!),
                        x.Subscription.CompanyServiceAccounts.Where(x => x.Identity!.IdentityAssignedRoles.Any()).Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Select(ur => ur.UserRoleText))))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferUpdateUrlData?> GetUpdateUrlDataAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId)
            .Select(os => new OfferUpdateUrlData(
                os.Offer!.Name,
                (os.Offer.AppInstanceSetup != null && os.Offer.AppInstanceSetup!.IsSingleInstance),
                os.Offer.ProviderCompanyId == userCompanyId,
                os.RequesterId,
                os.CompanyId,
                os.OfferSubscriptionStatusId,
                os.AppSubscriptionDetail == null ?
                    null :
                    new OfferUpdateUrlSubscriptionDetailData(
                        os.AppSubscriptionDetail.Id,
                        os.AppSubscriptionDetail.AppInstance!.IamClient!.ClientClientId,
                        os.AppSubscriptionDetail.AppSubscriptionUrl)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyAppSubscriptionDetail(Guid detailId, Guid subscriptionId, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters)
    {
        var appSubscriptionDetail = new AppSubscriptionDetail(detailId, subscriptionId);
        initialize?.Invoke(appSubscriptionDetail);
        _context.Attach(appSubscriptionDetail);
        setParameters.Invoke(appSubscriptionDetail);
    }

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<OfferSubscriptionStatusData>?>> GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                _context.OfferSubscriptions
                    .AsNoTracking()
                    .Where(os =>
                        os.Offer!.OfferTypeId == offerTypeId &&
                        os.CompanyId == userCompanyId)
                    .GroupBy(os => os.CompanyId),
                null,
                os => new OfferSubscriptionStatusData(
                    os.OfferId,
                    os.Offer!.Name,
                    os.Offer.Provider,
                    os.OfferSubscriptionStatusId,
                    os.Offer.Documents
                        .Where(document =>
                            document.DocumentTypeId == documentTypeId &&
                            document.DocumentStatusId == DocumentStatusId.LOCKED)
                        .Select(document => document.Id).FirstOrDefault()))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetOfferSubscriptionDataForProcessIdAsync(Guid processId) =>
        _context.Processes
            .AsNoTracking()
            .Where(process => process.Id == processId)
            .Select(process => process.OfferSubscription!.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<TriggerProviderInformation?> GetTriggerProviderInformation(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new
            {
                RequesterCompany = x.Requester!.Identity!.Company,
                x.Requester.Email,
                OfferId = x.Offer!.Id,
                OfferName = x.Offer.Name,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.AutoSetupUrl,
                x.Offer.SalesManagerId,
                x.Offer.OfferTypeId,
                CompanyUserId = x.Requester.Id,
                IsSingleInstance = x.Offer.AppInstanceSetup != null && x.Offer.AppInstanceSetup.IsSingleInstance
            })
            .Select(x => new TriggerProviderInformation(
                x.OfferId,
                x.OfferName,
                x.AutoSetupUrl,
                new CompanyInformationData(
                    x.RequesterCompany!.Id,
                    x.RequesterCompany.Name,
                    x.RequesterCompany.Address!.CountryAlpha2Code,
                    x.RequesterCompany.BusinessPartnerNumber
                ),
                x.Email,
                x.OfferTypeId,
                x.SalesManagerId,
                x.CompanyUserId,
                x.IsSingleInstance
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<SubscriptionActivationData?> GetSubscriptionActivationDataByIdAsync(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .AsSplitQuery()
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new SubscriptionActivationData(
                x.OfferId,
                x.OfferSubscriptionStatusId,
                x.Offer!.OfferTypeId,
                x.Offer.Name,
                x.Requester!.Identity!.Company!.Name,
                x.Requester.Identity!.CompanyId,
                x.Requester.Email,
                x.Requester.Firstname,
                x.Requester.Lastname,
                x.RequesterId,
                x.Offer.AppInstanceSetup == null
                    ? new ValueTuple<bool, string?>()
                    : new ValueTuple<bool, string?>(x.Offer.AppInstanceSetup.IsSingleInstance, x.Offer.AppInstanceSetup.InstanceUrl),
                x.Offer.AppInstances.Select(ai => ai.Id),
                x.OfferSubscriptionProcessData != null,
                x.Offer.SalesManagerId,
                x.Offer.ProviderCompanyId
            ))
            .SingleOrDefaultAsync();

    public Task<(bool IsValidSubscriptionId, bool IsActive)> IsActiveOfferSubscription(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.Id == offerSubscriptionId)
            .Select(os => new ValueTuple<bool, bool>(
                true,
                os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<VerifyProcessData?> GetProcessStepData(Guid offerSubscriptionId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        _context.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.Id == offerSubscriptionId)
            .Select(x => new VerifyProcessData(
                x.Process,
                x.Process!.ProcessSteps
                    .Where(step =>
                        processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                        step.ProcessStepStatusId == ProcessStepStatusId.TODO)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionClientCreationData?> GetClientCreationData(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new OfferSubscriptionClientCreationData(
                x.Offer!.Id,
                x.Offer.OfferTypeId,
                x.OfferSubscriptionProcessData!.OfferUrl,
                x.Offer!.OfferTypeId == OfferTypeId.APP || x.Offer.TechnicalUserProfiles.Any()
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionTechnicalUserCreationData?> GetTechnicalUserCreationData(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new OfferSubscriptionTechnicalUserCreationData(
                x.Offer!.OfferTypeId == OfferTypeId.APP || x.Offer.TechnicalUserProfiles.Any(),
                x.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId,
                x.Offer.Name,
                x.Company!.Name,
                x.CompanyId,
                x.Company.BusinessPartnerNumber,
                x.Offer.OfferTypeId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(IEnumerable<(Guid TechnicalUserId, string? TechnicalClientId)> ServiceAccounts, string? ClientId, string? CallbackUrl, OfferSubscriptionStatusId Status)> GetTriggerProviderCallbackInformation(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new ValueTuple<IEnumerable<(Guid, string?)>, string?, string?, OfferSubscriptionStatusId>(
                    x.CompanyServiceAccounts.Select(sa => new ValueTuple<Guid, string?>(sa.Id, sa.ClientId)),
                    x.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId,
                    x.Offer!.ProviderCompany!.ProviderCompanyDetail!.AutoSetupCallbackUrl,
                    x.OfferSubscriptionStatusId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferSubscriptionProcessData CreateOfferSubscriptionProcessData(Guid offerSubscriptionId, string offerUrl) =>
        _context.OfferSubscriptionsProcessDatas.Add(new OfferSubscriptionProcessData(offerSubscriptionId, offerUrl)).Entity;

    /// <inheritdoc />
    public void RemoveOfferSubscriptionProcessData(Guid offerSubscriptionId) =>
        _context.Remove(new OfferSubscriptionProcessData(offerSubscriptionId, null!));

    /// <inheritdoc />
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId) =>
        _context.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.Id == offerSubscriptionId)
            .SelectMany(x => x.Process!.ProcessSteps)
            .Select(x => new ProcessStepData(
                x.ProcessStepTypeId,
                x.ProcessStepStatusId,
                x.Message))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool Exists, bool IsOfferProvider, bool OfferSubscriptionAlreadyLinked, OfferSubscriptionStatusId OfferSubscriptionStatus, Guid? SelfDescriptionDocumentId, Guid CompanyId, string? ProviderBpn)> CheckOfferSubscriptionWithOfferProvider(Guid subscriptionId, Guid offerProvidingCompanyId) =>
        _context.OfferSubscriptions
            .Where(x => x.Id == subscriptionId)
            .Select(os => new ValueTuple<bool, bool, bool, OfferSubscriptionStatusId, Guid?, Guid, string?>(
                true,
                os.Offer!.ProviderCompanyId == offerProvidingCompanyId,
                os.ConnectorAssignedOfferSubscriptions.Any(),
                os.OfferSubscriptionStatusId,
                os.Company!.SelfDescriptionDocumentId,
                os.CompanyId,
                os.Company.BusinessPartnerNumber
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData(bool? connectorIdSet, Guid companyId) =>
        _context.OfferSubscriptions
            .Where(os =>
                os.Offer!.ProviderCompanyId == companyId &&
                (os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE || os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING) &&
                (connectorIdSet == null || (connectorIdSet.Value ? os.ConnectorAssignedOfferSubscriptions.Any() : !os.ConnectorAssignedOfferSubscriptions.Any())))
            .Select(os => new OfferSubscriptionConnectorData(
                os.Id,
                os.Company!.Name,
                os.Offer!.Name,
                os.ConnectorAssignedOfferSubscriptions.Select(c => c.ConnectorId)
            ))
            .ToAsyncEnumerable();
}

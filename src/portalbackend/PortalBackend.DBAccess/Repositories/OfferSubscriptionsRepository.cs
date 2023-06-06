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
    public OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId) =>
        _context.OfferSubscriptions.Add(new OfferSubscription(Guid.NewGuid(), offerId, companyId, offerSubscriptionStatusId, requesterId, creatorId)).Entity;

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(string iamUserId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId statusId, Guid? offerId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                _context.Offers
                    .AsNoTracking()
                    .Where(os =>
                        os.OfferTypeId == offerTypeId &&
                        (!offerId.HasValue || os.Id == offerId.Value) &&
                        os.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId) &&
                        os.OfferSubscriptions.Any(x => x.OfferSubscriptionStatusId == statusId))
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
                        .Where(os => os.OfferSubscriptionStatusId == statusId)
                        .Select(s =>
                            new CompanySubscriptionStatusData(
                                s.CompanyId,
                                s.Company!.Name,
                                s.Id,
                                s.OfferSubscriptionStatusId,
                                s.Company.Address!.CountryAlpha2Code,
                                s.Company.BusinessPartnerNumber,
                                s.Requester!.Email,
                                s.Offer!.TechnicalUserProfiles.Any(tup => tup.UserRoles.Any()))),
                    Image = g.Documents
                        .Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId == DocumentStatusId.LOCKED)
                        .Select(document => document.Id)
                        .FirstOrDefault()
                })
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid SubscriptionId, OfferSubscriptionStatusId SubscriptionStatusId, Guid RequestorId, string? AppName, Guid CompanyUserId, RequesterData Requester)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId) =>
        _context.Offers
            .Where(app => app.Id == appId)
            .Select(app => new
            {
                App = app,
                OfferSubscription = app.OfferSubscriptions.SingleOrDefault(subscription => subscription.CompanyId == companyId),
            })
            .Select(x => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, RequesterData>(
                x.OfferSubscription!.Id,
                x.OfferSubscription.OfferSubscriptionStatusId,
                x.OfferSubscription.RequesterId,
                x.App.Name,
                x.App.ProviderCompany!.CompanyUsers.SingleOrDefault(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)!.Id,
                new RequesterData(x.OfferSubscription.Requester!.Email, x.OfferSubscription.Requester.Firstname, x.OfferSubscription.Requester.Lastname)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferSubscription? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId) =>
        _context.Offers
            .Where(app => app.Id == appId)
            .Select(app => new ValueTuple<OfferSubscription?, bool>(
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
                x.Offer!.ProviderCompany!.CompanyUsers.Where(cu => cu.IamUser!.UserEntityId == iamUserId)
                    .Select(cu => cu.Id).SingleOrDefault(),
                x.Offer.ProviderCompany.CompanyServiceAccounts
                    .Where(cu => cu.IamServiceAccount!.UserEntityId == iamUserId).Select(cu => cu.Id).SingleOrDefault(),
                x.Company!.Name,
                x.CompanyId,
                x.RequesterId,
                x.OfferId,
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
    public Task<(Guid OfferSubscriptionId, OfferSubscriptionStatusId OfferSubscriptionStatusId, Process? Process, IEnumerable<ProcessStepTypeId>? ProcessStepTypeIds)> GetOfferSubscriptionStateForCompanyAsync(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        _context.OfferSubscriptions.AsNoTracking()
            .Where(x => x.OfferId == offerId && x.CompanyId == companyId && x.Offer!.OfferTypeId == offerTypeId)
            .Select(x => new ValueTuple<Guid, OfferSubscriptionStatusId, Process?, IEnumerable<ProcessStepTypeId>?>(x.Id, x.OfferSubscriptionStatusId, x.Process, x.Process!.ProcessSteps.Where(step => step.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(step => step.ProcessStepTypeId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription> setOptionalParameters)
    {
        var offerSubscription = _context.Attach(new OfferSubscription(offerSubscriptionId, Guid.Empty, Guid.Empty, default, Guid.Empty, Guid.Empty)).Entity;
        setOptionalParameters.Invoke(offerSubscription);
        return offerSubscription;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid OfferId, Guid SubscriptionId, string? OfferName, string SubscriptionUrl, Guid LeadPictureId, string Provider)> GetAllBusinessAppDataForUserIdAsync(string iamUserId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(user => user.IamUser!.UserEntityId == iamUserId)
            .SelectMany(user => user.Company!.OfferSubscriptions.Where(subscription =>
                subscription.Offer!.UserRoles.Any(ur => ur.CompanyUsers.Any(cu => cu.Id == user.Id)) &&
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
    public Task<(bool Exists, bool IsUserOfCompany, OfferSubscriptionDetailData Details)> GetSubscriptionDetailsAsync(Guid offerId, Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds, bool forProvider) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId)
            .Select(os => new
            {
                UserCompany = forProvider ? os.Offer!.ProviderCompany : os.Company,
                OtherCompany = forProvider ? os.Company : os.Offer!.ProviderCompany,
                OfferName = os.Offer!.Name,
                os.OfferId,
                os.OfferSubscriptionStatusId,
                os.CompanyServiceAccounts
            })
            .Select(x => new ValueTuple<bool, bool, OfferSubscriptionDetailData>(
                true,
                x.UserCompany!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                new OfferSubscriptionDetailData(
                    x.OfferId,
                    x.OfferSubscriptionStatusId,
                    x.OfferName,
                    x.OtherCompany!.Name,
                    x.OtherCompany!.BusinessPartnerNumber,
                    x.OtherCompany.CompanyUsers.Where(cu => cu.Email != null && cu.UserRoles.Any(ur => userRoleIds.Contains(ur.Id))).Select(cu => cu.Email!),
                    x.CompanyServiceAccounts.Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.UserRoles.Select(x => x.UserRoleText))))))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferUpdateUrlData?> GetUpdateUrlDataAsync(Guid offerId, Guid subscriptionId, string iamUserId) =>
        _context.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId)
            .Select(os => new OfferUpdateUrlData(
                os.Offer!.Name,
                (os.Offer.AppInstanceSetup != null && os.Offer.AppInstanceSetup!.IsSingleInstance),
                os.Offer.ProviderCompany!.CompanyUsers.Any(x => x.IamUser!.UserEntityId == iamUserId),
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
    public Func<int, int, Task<Pagination.Source<OfferSubscriptionStatusData>?>> GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(string iamUserId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                _context.OfferSubscriptions
                    .AsNoTracking()
                    .Where(os =>
                        os.Offer!.OfferTypeId == offerTypeId &&
                        os.Company!.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserId))
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
                RequesterCompany = x.Requester!.Company,
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
                x.Requester!.Company!.Name,
                x.Requester.CompanyId,
                x.Requester.Email,
                x.Requester.Firstname,
                x.Requester.Lastname,
                x.RequesterId,
                x.Offer.AppInstanceSetup == null
                    ? new ValueTuple<bool, string?>()
                    : new ValueTuple<bool, string?>(x.Offer.AppInstanceSetup.IsSingleInstance, x.Offer.AppInstanceSetup.InstanceUrl),
                x.Offer.AppInstances.Select(ai => ai.Id),
                x.OfferSubscriptionProcessData != null,
                x.Offer.SalesManagerId
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
                    x.CompanyServiceAccounts.Select(sa => new ValueTuple<Guid, string?>(sa.Id, sa.IamServiceAccount!.ClientId)),
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
}

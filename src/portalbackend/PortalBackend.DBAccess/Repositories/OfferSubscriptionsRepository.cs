/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Implementation of <see cref="IOfferSubscriptionsRepository"/> accessing database with EF Core.
/// </summary>
/// <param name="dbContext"></param>
public class OfferSubscriptionsRepository(PortalDbContext dbContext) : IOfferSubscriptionsRepository
{
    /// <inheritdoc />
    public OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId) =>
        dbContext.OfferSubscriptions.Add(new OfferSubscription(Guid.NewGuid(), offerId, companyId, offerSubscriptionStatusId, requesterId, DateTimeOffset.UtcNow)).Entity;

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, IEnumerable<OfferSubscriptionStatusId> statusIds, Guid? offerId, string? companyName) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                dbContext.Offers
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
                    _ => null
                },
                g => new OfferCompanySubscriptionStatusData(
                    g.Id,
                    g.Name,
                    g.OfferSubscriptions
                        .Where(os =>
                            statusIds.Contains(os.OfferSubscriptionStatusId) &&
                            (companyName == null || EF.Functions.ILike(os.Company!.Name, $"%{companyName.EscapeForILike()}%")))
                        .Select(s =>
                            new CompanySubscriptionStatusData(
                                s.CompanyId,
                                s.Company!.Name,
                                s.Id,
                                s.OfferSubscriptionStatusId,
                                s.Company.Address!.CountryAlpha2Code,
                                s.Company.BusinessPartnerNumber,
                                s.Requester!.Email,
                                s.Offer!.TechnicalUserProfiles.Any(tup => tup.TechnicalUserProfileAssignedUserRoles.Any()),
                                s.DateCreated,
                                s.Process!.ProcessSteps
                                    .Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO)
                                    .Select(ps => new ValueTuple<ProcessStepTypeId, ProcessStepStatusId>(
                                        ps.ProcessStepTypeId,
                                        ps.ProcessStepStatusId))
                                    .Distinct())),
                    g.Documents
                        .Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId == DocumentStatusId.LOCKED)
                        .Select(document => document.Id)
                        .FirstOrDefault()
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(OfferSubscriptionStatusId OfferSubscriptionStatusId, bool IsSubscribingCompany, bool IsValidSubscriptionId, IEnumerable<Guid> ConnectorIds, IEnumerable<Guid> ServiceAccounts)> GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(Guid subscriptionId, Guid userCompanyId) =>
        dbContext.OfferSubscriptions
            .Where(os =>
                os.Id == subscriptionId
            )
            .Select(os => new ValueTuple<OfferSubscriptionStatusId, bool, bool, IEnumerable<Guid>, IEnumerable<Guid>>(
                os.OfferSubscriptionStatusId,
                os.CompanyId == userCompanyId,
                true,
                os.ConnectorAssignedOfferSubscriptions.Where(caos => caos.Connector!.StatusId != ConnectorStatusId.INACTIVE).Select(caos =>
                    caos.Connector!.Id),
                os.ConnectorAssignedOfferSubscriptions.Where(caos => caos.Connector!.TechnicalUserId != null && caos.Connector.TechnicalUser!.Identity!.UserStatusId != UserStatusId.INACTIVE).Select(caos =>
                    caos.Connector!.TechnicalUser!.Identity!.Id)
            ))
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, bool IsValidOfferSubscription)> GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(Guid subscriptionId, Guid userId, OfferTypeId offerTypeId) =>
        dbContext.CompanyUsers
            .Where(user => user.Id == userId)
            .Select(user => user.Identity!.Company)
            .Select(company => new ValueTuple<Guid, bool>(
                company!.Id,
                company.OfferSubscriptions.Any(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<SubscriptionDetailData?> GetSubscriptionDetailDataForOwnUserAsync(Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId) =>
        dbContext.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.Offer!.OfferTypeId == offerTypeId && os.CompanyId == userCompanyId)
            .Select(os => new SubscriptionDetailData(os.OfferId, os.Offer!.Name!, os.OfferSubscriptionStatusId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionTransferData?> GetOfferDetailsAndCheckProviderCompany(Guid offerSubscriptionId, Guid providerCompanyId, OfferTypeId offerTypeId) =>
        dbContext.OfferSubscriptions
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
                new ValueTuple<bool, string?>(x.Offer.AppInstanceSetup != null && x.Offer.AppInstanceSetup.IsSingleInstance, x.Offer.AppInstanceSetup!.InstanceUrl),
                x.Offer.AppInstances.Select(ai => ai.Id),
                x.Offer.SalesManagerId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<bool> CheckPendingOrActiveSubscriptionExists(Guid offerId, Guid companyId, OfferTypeId offerTypeId) =>
        dbContext.OfferSubscriptions.AsNoTracking()
            .AnyAsync(x =>
                x.OfferId == offerId &&
                x.CompanyId == companyId &&
                x.Offer!.OfferTypeId == offerTypeId &&
                (x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE || x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING));

    /// <inheritdoc />
    public OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription> setOptionalParameters)
    {
        var offerSubscription = dbContext.Attach(new OfferSubscription(offerSubscriptionId, Guid.Empty, Guid.Empty, default, Guid.Empty, default)).Entity;
        setOptionalParameters.Invoke(offerSubscription);
        return offerSubscription;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid OfferId, Guid SubscriptionId, string? OfferName, string SubscriptionUrl, Guid LeadPictureId, string Provider)> GetAllBusinessAppDataForUserIdAsync(Guid userId) =>
        dbContext.CompanyUsers.AsNoTracking()
            .Where(user => user.Id == userId && user.Identity!.IdentityTypeId == IdentityTypeId.COMPANY_USER)
            .SelectMany(user => user.Identity!.Company!.OfferSubscriptions.Where(subscription =>
                subscription.Offer!.OfferTypeId == OfferTypeId.APP &&
                subscription.Offer.UserRoles.Any(ur => ur.IdentityAssignedRoles.Any(iar => iar.IdentityId == userId)) &&
                subscription.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                subscription.AppSubscriptionDetail!.AppInstance != null &&
                subscription.AppSubscriptionDetail.AppSubscriptionUrl != null))
            .Select(offerSubscription => new ValueTuple<Guid, Guid, string?, string, Guid, string>(
                offerSubscription.OfferId,
                offerSubscription.Id,
                offerSubscription.Offer!.Name,
                offerSubscription.AppSubscriptionDetail!.AppSubscriptionUrl!,
                offerSubscription.Offer!.Documents.Where(document => document.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && document.DocumentStatusId != DocumentStatusId.INACTIVE).Select(document => document.Id).FirstOrDefault(),
                offerSubscription.Offer!.ProviderCompany!.Name
            )).ToAsyncEnumerable();

    public Task<(bool Exists, bool IsUserOfCompany, OfferProviderSubscriptionDetail? Details)> GetOfferSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds) =>
        dbContext.OfferSubscriptions
            .AsSplitQuery()
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId && os.Offer!.OfferTypeId == offerTypeId)
            .Select(os => new
            {
                IsProviderCompany = os.Offer!.ProviderCompanyId == userCompanyId,
                Subscription = os,
                Company = os.Company
            })
            .Select(x => new ValueTuple<bool, bool, OfferProviderSubscriptionDetail?>(
                true,
                x.IsProviderCompany,
                x.IsProviderCompany
                    ? new OfferProviderSubscriptionDetail(
                        x.Subscription.OfferId,
                        x.Subscription.OfferSubscriptionStatusId,
                        x.Subscription.Offer!.Name,
                        x.Company!.Name,
                        x.Company.BusinessPartnerNumber,
                        x.Company.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(id => id.CompanyUser!).Where(cu => cu.Email != null && cu.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Any(ur => userRoleIds.Contains(ur.Id))).Select(cu => cu.Email!),
                        x.Subscription.Technicalusers.Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Select(ur => ur.UserRoleText))),
                        offerTypeId == OfferTypeId.APP ? x.Subscription.AppSubscriptionDetail!.AppSubscriptionUrl : null,
                        offerTypeId == OfferTypeId.APP ? x.Subscription.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId : null,
                        x.Subscription.Process!.ProcessSteps
                            .Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO)
                            .Select(ps => new ValueTuple<ProcessStepTypeId, ProcessStepStatusId>(
                                ps.ProcessStepTypeId,
                                ps.ProcessStepStatusId))
                            .Distinct(),
                        x.Subscription.ConnectorAssignedOfferSubscriptions.Select(c => new SubscriptionAssignedConnectorData(c.ConnectorId, c.Connector!.Name, c.Connector.ConnectorUrl)),
                        x.Company.CompanyWalletData == null ? null : new ExternalServiceData(x.Company.CompanyWalletData!.Did, x.Company.BusinessPartnerNumber, x.Company.CompanyWalletData.AuthenticationServiceUrl))
                    : null))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, bool IsUserOfCompany, SubscriberSubscriptionDetailData? Details)> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds) =>
        dbContext.OfferSubscriptions
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
                        x.Subscription.Technicalusers.Where(x => x.Identity!.IdentityAssignedRoles.Any()).Select(sa => new SubscriptionTechnicalUserData(sa.Id, sa.Name, sa.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Select(ur => ur.UserRoleText))),
                        x.Subscription.ConnectorAssignedOfferSubscriptions.Select(caos => new SubscriptionAssignedConnectorData(
                            caos.Connector!.Id,
                            caos.Connector.Name,
                            caos.Connector.ConnectorUrl)))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferUpdateUrlData?> GetUpdateUrlDataAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId) =>
        dbContext.OfferSubscriptions
            .Where(os => os.Id == subscriptionId && os.OfferId == offerId)
            .Select(os => new OfferUpdateUrlData(
                os.Offer!.Name,
                os.Offer.AppInstanceSetup != null && os.Offer.AppInstanceSetup!.IsSingleInstance,
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
        dbContext.Attach(appSubscriptionDetail);
        setParameters.Invoke(appSubscriptionDetail);
    }

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<OfferSubscriptionStatusData>?>>
        GetOwnCompanySubscribedOfferSubscriptionStatusAsync(Guid userCompanyId, OfferTypeId offerTypeId,
            DocumentTypeId documentTypeId, OfferSubscriptionStatusId? statusId, string? name) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
                skip,
                take,
                dbContext.OfferSubscriptions
                    .AsNoTracking()
                    .Where(os =>
                        os.Offer!.OfferTypeId == offerTypeId &&
                        os.CompanyId == userCompanyId &&
                        (statusId == null || os.OfferSubscriptionStatusId == statusId) &&
                        (name == null || (os.Offer.Name != null && EF.Functions.ILike(os.Offer!.Name, $"%{name.EscapeForILike()}%"))))
                    .GroupBy(os => os.CompanyId),
                null,
                os => new OfferSubscriptionStatusData(
                    os.OfferId,
                    os.Offer!.Name,
                    os.Offer.ProviderCompany!.Name,
                    os.OfferSubscriptionStatusId,
                    os.Id,
                    os.Offer.Documents
                        .Where(document =>
                            document.DocumentTypeId == documentTypeId &&
                            document.DocumentStatusId == DocumentStatusId.LOCKED)
                        .Select(document => document.Id).FirstOrDefault()))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetOfferSubscriptionDataForProcessIdAsync(Guid processId) =>
        dbContext.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.ProcessId == processId)
            .Select(os => os.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<TriggerProviderInformation?> GetTriggerProviderInformation(Guid offerSubscriptionId) =>
         dbContext.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new
            {
                RequesterCompany = x.Requester!.Identity!.Company,
                x.Requester.Email,
                OfferId = x.Offer!.Id,
                OfferName = x.Offer.Name,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.AutoSetupUrl,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.AuthUrl,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.ClientId,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.ClientSecret,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.InitializationVector,
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.EncryptionMode,
                x.Offer.SalesManagerId,
                x.Offer.OfferTypeId,
                CompanyUserId = x.Requester.Id,
                IsSingleInstance = x.Offer.AppInstanceSetup != null && x.Offer.AppInstanceSetup.IsSingleInstance
            })
            .Select(x => new TriggerProviderInformation(
                x.OfferId,
                x.OfferName,
                x.AutoSetupUrl,
               x.AuthUrl == null ? null : new ProviderAuthInformation(
                    x.AuthUrl,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode
                ),
                new CompanyInformationData(
                    x.RequesterCompany!.Id,
                    x.RequesterCompany.Name,
                    x.RequesterCompany.Address!.CountryAlpha2Code,
                    x.RequesterCompany.BusinessPartnerNumber,
                    x.Email
                ),
                x.OfferTypeId,
                x.SalesManagerId,
                x.CompanyUserId,
                x.IsSingleInstance
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<SubscriptionActivationData?> GetSubscriptionActivationDataByIdAsync(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
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
                new ValueTuple<bool, string?>(x.Offer.AppInstanceSetup != null && x.Offer.AppInstanceSetup.IsSingleInstance, x.Offer.AppInstanceSetup!.InstanceUrl),
                x.Offer!.AppInstances.Select(ai => ai.Id),
                x.OfferSubscriptionProcessData!.Id,
                x.Offer.SalesManagerId,
                x.Offer.ProviderCompanyId,
                x.Offer.OfferTypeId == OfferTypeId.APP && (x.Offer.AppInstanceSetup == null || !x.Offer.AppInstanceSetup!.IsSingleInstance) ?
                    x.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId :
                    null,
                x.Technicalusers.Where(sa => sa.TechnicalUserKindId == TechnicalUserKindId.INTERNAL && sa.ClientClientId != null).Select(sa => sa.ClientClientId!),
                x.Offer.ProviderCompany!.ProviderCompanyDetail!.AutoSetupCallbackUrl != null
            ))
            .SingleOrDefaultAsync();

    public Task<(bool IsValidSubscriptionId, bool IsActive)> IsActiveOfferSubscription(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.Id == offerSubscriptionId)
            .Select(os => new ValueTuple<bool, bool>(
                true,
                os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?> GetProcessStepData(Guid offerSubscriptionId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        dbContext.OfferSubscriptions
            .AsNoTracking()
            .Where(os => os.Id == offerSubscriptionId)
            .Select(x => new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                x.Process,
                x.Process!.ProcessSteps
                    .Where(step =>
                        processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                        step.ProcessStepStatusId == ProcessStepStatusId.TODO)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<OfferSubscriptionClientCreationData?> GetClientCreationData(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
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
        dbContext.OfferSubscriptions
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
    public Task<(IEnumerable<(Guid TechnicalUserId, string? TechnicalClientId, TechnicalUserKindId TechnicalUserKindId)> ServiceAccounts, string? ClientId, string? CallbackUrl, ProviderAuthInformation? AuthDetails, OfferSubscriptionStatusId Status)> GetTriggerProviderCallbackInformation(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new ValueTuple<IEnumerable<(Guid, string?, TechnicalUserKindId)>, string?, string?, ProviderAuthInformation?, OfferSubscriptionStatusId>(
                    x.Technicalusers.Select(sa => new ValueTuple<Guid, string?, TechnicalUserKindId>(sa.Id, sa.ClientClientId, sa.TechnicalUserKindId)),
                    x.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId,
                    x.Offer!.ProviderCompany!.ProviderCompanyDetail!.AutoSetupCallbackUrl,
                    x.Offer.ProviderCompany.ProviderCompanyDetail!.AuthUrl == null ? null : new ProviderAuthInformation(x.Offer!.ProviderCompany!.ProviderCompanyDetail!.AuthUrl,
                                                 x.Offer!.ProviderCompany!.ProviderCompanyDetail!.ClientId,
                                                 x.Offer!.ProviderCompany!.ProviderCompanyDetail!.ClientSecret,
                                                 x.Offer!.ProviderCompany!.ProviderCompanyDetail!.InitializationVector,
                                                 x.Offer!.ProviderCompany!.ProviderCompanyDetail!.EncryptionMode
                                                 ),
                    x.OfferSubscriptionStatusId
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public OfferSubscriptionProcessData CreateOfferSubscriptionProcessData(Guid offerSubscriptionId, string offerUrl) =>
        dbContext.OfferSubscriptionsProcessDatas.Add(new OfferSubscriptionProcessData(Guid.NewGuid(), offerSubscriptionId, offerUrl)).Entity;

    /// <inheritdoc />
    public void RemoveOfferSubscriptionProcessData(Guid offerSubscriptionProcessDataId) =>
        dbContext.Remove(new OfferSubscriptionProcessData(offerSubscriptionProcessDataId, Guid.Empty, null!));

    /// <inheritdoc />
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
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
        dbContext.OfferSubscriptions
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
        dbContext.OfferSubscriptions
            .Where(os =>
                os.Offer!.ProviderCompanyId == companyId && os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                (connectorIdSet == null || (connectorIdSet.Value ? os.ConnectorAssignedOfferSubscriptions.Any() : !os.ConnectorAssignedOfferSubscriptions.Any())))
            .Select(os => new OfferSubscriptionConnectorData(
                os.Id,
                os.Company!.Name,
                os.Offer!.Name,
                os.ConnectorAssignedOfferSubscriptions.Select(c => c.ConnectorId)
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<ActiveOfferSubscriptionStatusData> GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId) =>
        dbContext.OfferSubscriptions
            .AsNoTracking()
            .Where(os =>
                os.Offer!.OfferTypeId == offerTypeId && os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                os.CompanyId == userCompanyId)
            .Select(os => new ActiveOfferSubscriptionStatusData(
                os.OfferId,
                os.Offer!.Name,
                os.Offer.ProviderCompany!.Name,
                os.Offer.Documents
                    .Where(document =>
                        document.DocumentTypeId == documentTypeId
                        && document.DocumentStatusId == DocumentStatusId.LOCKED)
                    .Select(document => document.Id).FirstOrDefault(),
                os.Id
            )).ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<OfferSubscriptionData> GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId) =>
        dbContext.OfferSubscriptions
            .AsNoTracking()
            .Where(os =>
                os.Offer!.OfferTypeId == offerTypeId && os.OfferSubscriptionStatusId != OfferSubscriptionStatusId.INACTIVE &&
                os.CompanyId == userCompanyId)
            .Select(os => new OfferSubscriptionData(
                os.OfferId,
                os.OfferSubscriptionStatusId
            )).ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<bool> CheckOfferSubscriptionForProvider(Guid offerSubscriptionId, Guid providerCompanyId) =>
        dbContext.OfferSubscriptions
            .Where(x =>
                x.Id == offerSubscriptionId &&
                x.Offer!.ProviderCompanyId == providerCompanyId)
            .AnyAsync();

    public Task<(string? Bpn, string? OfferName, Guid? ProcessId)> GetDimTechnicalUserDataForSubscriptionId(Guid offerSubscriptionId) =>
        dbContext.OfferSubscriptions
            .Where(x => x.Id == offerSubscriptionId)
            .Select(x => new ValueTuple<string?, string?, Guid?>(
                x.Company!.BusinessPartnerNumber,
                x.Offer!.Name,
                x.ProcessId))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Process, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>)> GetOfferSubscriptionRetriggerProcessesForCompanyId(Guid companyId) =>
        dbContext.ProcessSteps
            .AsNoTracking()
            .Where(ps =>
                ps.ProcessStepStatusId == ProcessStepStatusId.TODO &&
                ps.ProcessStepTypeId == ProcessStepTypeId.RETRIGGER_PROVIDER &&
                ps.Process!.OfferSubscription!.Offer!.ProviderCompanyId == companyId)
            .OrderBy(ps => ps.ProcessId)
            .Select(ps => new ValueTuple<Process, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>(ps.Process!, ps))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(ProcessTypeId ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId> ProcessData, Guid? TechnicalUserId, Guid? TechnicalUserVersion)> GetProcessDataForTechnicalUserCallback(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        dbContext.TechnicalUsers
            .AsNoTracking()
            .Where(t => t.OfferSubscription!.ProcessId == processId && t.TechnicalUserKindId == TechnicalUserKindId.EXTERNAL)
            .Select(x => new ValueTuple<ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>, Guid?, Guid?>(
                    x.OfferSubscription!.Process!.ProcessTypeId,
                    new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
                        x.OfferSubscription.Process,
                        x.OfferSubscription.Process.ProcessSteps
                            .Where(step =>
                                processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                                step.ProcessStepStatusId == ProcessStepStatusId.TODO)),
                    x.Id,
                    x.Version))
            .ToAsyncEnumerable();
}

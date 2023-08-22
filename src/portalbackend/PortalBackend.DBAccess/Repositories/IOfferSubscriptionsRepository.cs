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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing company assigned apps on persistence layer.
/// </summary>
public interface IOfferSubscriptionsRepository
{
    /// <summary>
    /// Adds the given company assigned app to the database
    /// </summary>
    /// <param name="offerId">Id of the assigned app</param>
    /// <param name="companyId">Id of the company</param>
    /// <param name="offerSubscriptionStatusId">id of the app subscription status</param>
    /// <param name="requesterId">id of the user that requested the subscription of the app</param>
    /// <param name="creatorId">id of the creator</param>
    OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId);

    /// <summary>
    /// Gets the provided offer subscription statuses for the user and given company
    /// </summary>
    /// <param name="userCompanyId"></param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <param name="sorting"></param>
    /// <param name="statusIds"></param>
    /// <returns>Returns a func with skip, take and the pagination of the source</returns>
    Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, IEnumerable<OfferSubscriptionStatusId> statusIds, Guid? offerId);

    Task<(OfferSubscriptionStatusId SubscriptionStatusId, Guid RequestorId, Guid AppId, string? AppName, bool IsUserOfProvider, RequesterData Requester)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid subscriptionId, Guid userCompanyId);

    Task<(OfferSubscriptionStatusId OfferSubscriptionStatusId, bool IsSubscribingCompany, bool IsValidSubscriptionId, IEnumerable<Guid> ConnectorIds, IEnumerable<Guid> ServiceAccounts)> GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(Guid subscriptionId, Guid userCompanyId);

    Task<(Guid companyId, OfferSubscription? offerSubscription)> GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(Guid subscriptionId, Guid userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the subscription detail data for the given id and user
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="userCompanyId">the users company id</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>returns the subscription detail data if found</returns>
    Task<SubscriptionDetailData?> GetSubscriptionDetailDataForOwnUserAsync(Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer details for the given id.
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription.</param>
    /// <param name="providerCompanyId">Id of the provider company.</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the offer details.</returns>
    Task<OfferSubscriptionTransferData?> GetOfferDetailsAndCheckProviderCompany(Guid offerSubscriptionId, Guid providerCompanyId, OfferTypeId offerTypeId);

    public Task<bool> CheckPendingOrActiveSubscriptionExists(Guid offerId, Guid companyId, OfferTypeId offerTypeId);

    OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription> setOptionalParameters);

    /// <summary>
    /// Gets all business app data for the given userId
    /// </summary>
    /// <param name="userId">Id of the user to get the app data for.</param>
    /// <returns>Returns an IAsyncEnumerable of app data</returns>
    IAsyncEnumerable<(Guid OfferId, Guid SubscriptionId, string? OfferName, string SubscriptionUrl, Guid LeadPictureId, string Provider)> GetAllBusinessAppDataForUserIdAsync(Guid userId);

    /// <summary>
    /// Gets the offer details for the offer subscription for a provider company user
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="userCompanyId">Id of the user company</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="userRoleIds">Ids of the user roles the contacts should be in</param>
    /// <returns>Returns details for the offer subscription</returns>
    Task<(bool Exists, bool IsUserOfCompany, ProviderSubscriptionDetailData? Details)> GetSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds);

    /// <summary>
    /// Gets the app details for the offer subscription for a provider company user
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="userCompanyId">Id of the user company</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="userRoleIds">Ids of the user roles the contacts should be in</param>
    /// <returns>Returns details for the offer subscription</returns>
    Task<(bool Exists, bool IsUserOfCompany, AppProviderSubscriptionDetailData? Details)> GetAppSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds);

    /// <summary>
    /// Gets the offer details for the offer subscription for a subscribing company user
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="userCompanyId">Id of the user company</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="userRoleIds">Ids of the user roles the contacts should be in</param>
    /// <returns>Returns details for the offer subscription</returns>
    Task<(bool Exists, bool IsUserOfCompany, SubscriberSubscriptionDetailData? Details)> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId, OfferTypeId offerTypeId, IEnumerable<Guid> userRoleIds);

    /// <summary>
    /// Get the data to update the subscription url
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="userCompanyId">Id of the user company</param>
    /// <returns>Returns the data needed to update the subscription url</returns>
    Task<OfferUpdateUrlData?> GetUpdateUrlDataAsync(Guid offerId, Guid subscriptionId, Guid userCompanyId);

    /// <summary>
    /// The subscription details
    /// </summary>
    /// <param name="detailId">Id of the detail to update</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="initialize">Initializes the entity</param>
    /// <param name="setParameters">Updates the fields</param>
    void AttachAndModifyAppSubscriptionDetail(Guid detailId, Guid subscriptionId, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters);

    /// <summary>
    /// Gets the Service offer subscription statuses for the user
    /// </summary>
    /// <param name="userCompanyId">Id of users company</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <param name="documentTypeId">Id of the document type</param>
    /// <returns>Returns a func with skip, take and the pagination of the source</returns>
    Func<int, int, Task<Pagination.Source<OfferSubscriptionStatusData>?>> GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processId">Id of the process</param>
    /// <returns>Returns offer subscription process data</returns>
    Task<Guid> GetOfferSubscriptionDataForProcessIdAsync(Guid processId);

    Task<TriggerProviderInformation?> GetTriggerProviderInformation(Guid offerSubscriptionId);
    Task<SubscriptionActivationData?> GetSubscriptionActivationDataByIdAsync(Guid offerSubscriptionId);
    Task<(bool IsValidSubscriptionId, bool IsActive)> IsActiveOfferSubscription(Guid offerSubscriptionId);
    Task<VerifyProcessData?> GetProcessStepData(Guid offerSubscriptionId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    Task<OfferSubscriptionClientCreationData?> GetClientCreationData(Guid offerSubscriptionId);
    Task<OfferSubscriptionTechnicalUserCreationData?> GetTechnicalUserCreationData(Guid offerSubscriptionId);
    Task<(IEnumerable<(Guid TechnicalUserId, string? TechnicalClientId)> ServiceAccounts, string? ClientId, string? CallbackUrl, OfferSubscriptionStatusId Status)> GetTriggerProviderCallbackInformation(Guid offerSubscriptionId);
    OfferSubscriptionProcessData CreateOfferSubscriptionProcessData(Guid offerSubscriptionId, string offerUrl);
    void RemoveOfferSubscriptionProcessData(Guid offerSubscriptionId);
    IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription(Guid offerSubscriptionId);
    Task<(bool Exists, bool IsOfferProvider, bool OfferSubscriptionAlreadyLinked, OfferSubscriptionStatusId OfferSubscriptionStatus, Guid? SelfDescriptionDocumentId, Guid CompanyId, string? ProviderBpn)> CheckOfferSubscriptionWithOfferProvider(Guid subscriptionId, Guid offerProvidingCompanyId);
    IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData(bool? connectorIdSet, Guid companyId);

    /// <summary>
    /// Gets the apps Active offer subscription statuses for the user
    /// </summary>
    /// <param name="userCompanyId">Id of users company</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <param name="documentTypeId">Id of the document type</param>
    /// <returns>apps offer subscription statuses for the user</returns>
    IAsyncEnumerable<ActiveOfferSubscriptionStatusData> GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId);

    /// <summary>
    /// Gets the apps offer subscription statuses for the user
    /// </summary>
    /// <param name="userCompanyId">Id of users company</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>apps offer subscription statuses for the user</returns>
    IAsyncEnumerable<OfferSubscriptionData> GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync(Guid userCompanyId, OfferTypeId offerTypeId);
}

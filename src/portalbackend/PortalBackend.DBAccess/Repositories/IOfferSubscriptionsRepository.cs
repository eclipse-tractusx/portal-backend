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

using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
    OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId);

    /// <summary>
    ///
    /// </summary>
    /// <param name="iamUserId"></param>
    IAsyncEnumerable<AppWithSubscriptionStatus> GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the provided offer subscription statuses for the user and given company
    /// </summary>
    /// <param name="iamUserId">Id of user of the Providercompany</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <param name="sorting"></param>
    /// <param name="statusId"></param>
    /// <returns>Returns a func with skip, take and the pagination of the source</returns>
    Func<int, int, Task<Pagination.Source<OfferCompanySubscriptionStatusData>?>> GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(string iamUserId, OfferTypeId offerTypeId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId);

    Task<(OfferSubscription? companyAssignedApp, bool isMemberOfCompanyProvidingApp, string? appName, Guid companyUserId)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId);

    Task<(OfferSubscription? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId);

    Task<(Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId)> GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the subscription detail data for the given id and user
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="iamUserId">the iam user id</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>returns the subscription detail data if found</returns>
    Task<SubscriptionDetailData?> GetSubscriptionDetailDataForOwnUserAsync(Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer details for the given id.
    /// </summary>
    /// <param name="offerSubscriptionId">Id of the offer subscription.</param>
    /// <param name="iamUserId">Id of the iamUser.</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the offer details.</returns>
    Task<OfferSubscriptionTransferData?> GetOfferDetailsAndCheckUser(Guid offerSubscriptionId, string iamUserId, OfferTypeId offerTypeId);

    public Task<(Guid offerSubscriptionId, OfferSubscriptionStatusId offerSubscriptionStatusId)> GetOfferSubscriptionStateForCompanyAsync(Guid offerId, Guid companyId, OfferTypeId offerTypeId);
    
    OfferSubscription AttachAndModifyOfferSubscription(Guid offerSubscriptionId, Action<OfferSubscription>? setOptionalParameters = null);

    /// <summary>
    /// Gets all business app data for the given userId
    /// </summary>
    /// <param name="iamUserId">Id of the user to get the app data for.</param>
    /// <returns>Returns an IAsyncEnumerable of app data</returns>
    IAsyncEnumerable<(Guid SubscriptionId, string? OfferName, string SubscriptionUrl, string? ThumbnailUrl, string Provider)> GetAllBusinessAppDataForUserIdAsync(string iamUserId);
}

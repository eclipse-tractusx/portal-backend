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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

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
    OfferSubscription CreateOfferSubscription(Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId);

    IQueryable<CompanyUser> GetOwnCompanyAppUsersUntrackedAsync(Guid appId, string iamUserId, string? firstName = null, string? lastName = null, string? email = null,string? roleName = null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="iamUserId"></param>
    IAsyncEnumerable<AppWithSubscriptionStatus> GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the provided app subscription statuses for the user and given company
    /// </summary>
    /// <param name="iamUserId">Id of user of the Providercompany</param>
    /// <returns>Returns a IAsyncEnumerable of the found <see cref="AppCompanySubscriptionStatusData"/></returns>
    IAsyncEnumerable<AppCompanySubscriptionStatusData> GetOwnCompanyProvidedAppSubscriptionStatusesUntrackedAsync(string iamUserId);

    Task<(OfferSubscription? companyAssignedApp, bool isMemberOfCompanyProvidingApp, string? appName, Guid companyUserId)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId);

    Task<(OfferSubscription? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId);

    Task<(Guid companyId, OfferSubscription? offerSubscription, string companyName, Guid companyUserId)> GetCompanyIdWithAssignedOfferForCompanyUserAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId);
    
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
}

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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

/// <summary>
/// Business logic for handling offer-related operations. Includes persistence layer access.
/// </summary>
public interface IOfferSetupService
{
    /// <summary>
    /// Internally auto setup the offer.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="itAdminRoles">Roles that will be assigned to the company admin</param>
    /// <param name="identity">Identity of the user</param>
    /// <param name="offerTypeId">OfferTypeId of offer to be created</param>
    /// <param name="basePortalAddress">Address of the portal</param>
    /// <param name="serviceManagerRoles">Roles of the service Managers</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupOfferAsync(OfferAutoSetupData data, IEnumerable<UserRoleConfig> itAdminRoles, (Guid UserId, Guid CompanyId) identity, OfferTypeId offerTypeId, string basePortalAddress, IEnumerable<UserRoleConfig> serviceManagerRoles);

    /// <summary>
    /// Setup a single instance app
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    /// <param name="instanceUrl">Url for the offer instance</param>
    Task SetupSingleInstance(Guid offerId, string instanceUrl);

    /// <summary>
    /// Deletes the client and the app instance
    /// </summary>
    /// <param name="appInstanceId">id of the app instance</param>
    /// <param name="clientId">Id of the iamClient</param>
    /// <param name="clientClientId">Id of the client</param>
    Task DeleteSingleInstance(Guid appInstanceId, Guid clientId, string clientClientId);

    /// <summary>
    /// Setup a single instance app
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    Task<IEnumerable<string?>> ActivateSingleInstanceAppAsync(Guid offerId);

    /// <summary>
    /// Updates the single instance
    /// </summary>
    /// <param name="clientClientId">internal client id</param>
    /// <param name="instanceUrl">the new instance url</param>
    Task UpdateSingleInstance(string clientClientId, string instanceUrl);

    /// <summary>
    /// Internally auto setup the offer.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="companyId">Id of the company</param>
    /// <param name="offerTypeId">OfferTypeId of offer to be created</param>
    Task StartAutoSetupAsync(OfferAutoSetupData data, Guid companyId, OfferTypeId offerTypeId);

    /// <summary>
    /// Creates the single instance subscription detail and creates the activation step.
    /// </summary>
    /// <param name="offerSubscriptionId">The offer subscription id and url for the service</param>
    /// <param name="companyId">Id of the company</param>
    Task CreateSingleInstanceSubscriptionDetail(Guid offerSubscriptionId, Guid companyId);

    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateClient(Guid offerSubscriptionId);

    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateTechnicalUser(Guid offerSubscriptionId, IEnumerable<UserRoleConfig> itAdminRoles);

    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> ActivateSubscription(Guid offerSubscriptionId, IEnumerable<UserRoleConfig> itAdminRoles, IEnumerable<UserRoleConfig> serviceManagerRoles, string basePortalAddress);
}

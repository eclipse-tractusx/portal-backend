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

using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Service.Service.BusinessLogic;

/// <summary>
/// Business logic for handling service-related operations. Includes persistence layer access.
/// </summary>
public interface IServiceBusinessLogic
{ 
    /// <summary>
    /// Gets all active services from the database
    /// </summary>
    /// <returns>All services with pagination</returns>
    Task<Pagination.Response<ServiceDetailData>> GetAllActiveServicesAsync(int page, int size);

    /// <summary>
    /// Creates a new service offering
    /// </summary>
    /// <param name="data">The data to create the service offering</param>
    /// <param name="iamUserId">the iamUser id</param>
    /// <returns>The id of the newly created service</returns>
    Task<Guid> CreateServiceOffering(ServiceOfferingData data, string iamUserId);

    /// <summary>
    /// Adds a subscription to the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the users company should be subscribed to</param>
    /// <param name="iamUserId">Id of the user</param>
    /// <returns></returns>
    Task AddServiceSubscription(Guid serviceId, string iamUserId);

    /// <summary>
    /// Gets the service detail data for the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the details should be retrieved for.</param>
    /// <param name="lang">Shortcode of the language for the text translations</param>
    /// <returns>Returns the service detail data</returns>
    Task<ServiceDetailData> GetServiceDetailsAsync(Guid serviceId, string lang);

    /// <summary>
    /// Creates new service agreement consents with the given data for the given service
    /// </summary>
    /// <param name="serviceId">Id of the service to create the consents for.</param>
    /// <param name="serviceAgreementConsentData">service agreement consents</param>
    /// <param name="iamUserId">Id of the iam user</param>
    Task<Guid> CreateServiceAgreementConsent(Guid serviceId, ServiceAgreementConsentData serviceAgreementConsentData,
        string iamUserId);

    /// <summary>
    /// Gets the service agreement data
    /// </summary>
    /// <param name="iamUserId">id of the iam user</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetServiceAgreement(string iamUserId);
}

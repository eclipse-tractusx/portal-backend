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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Business logic for handling service-related operations. Includes persistence layer access.
/// </summary>
public interface IServiceBusinessLogic
{ 
    /// <summary>
    /// Gets all active services from the database
    /// </summary>
    /// <returns>All services with pagination</returns>
    Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync(int page, int size, ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId);

    /// <summary>
    /// Creates a new service offering
    /// </summary>
    /// <param name="data">The data to create the service offering</param>
    /// <param name="iamUserId">the iamUser id</param>
    /// <returns>The id of the newly created service</returns>
    Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, string iamUserId);

    /// <summary>
    /// Adds a subscription to the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the users company should be subscribed to</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <param name="iamUserId">Id of the user</param>
    /// <param name="accessToken">The access token of the user</param>
    /// <returns></returns>
    Task<Guid> AddServiceSubscription(Guid serviceId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, string accessToken);

    /// <summary>
    /// Gets the service detail data for the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the details should be retrieved for.</param>
    /// <param name="lang">Shortcode of the language for the text translations</param>
    /// <param name="iamUserId">Id of the iam User</param>
    /// <returns>Returns the service detail data</returns>
    Task<ServiceDetailData> GetServiceDetailsAsync(Guid serviceId, string lang, string iamUserId);

    /// <summary>
    /// Gets the Subscription Details for the given Id
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="iamUserId">Id of the user</param>
    /// <returns>Returns the details for the subscription</returns>
    Task<SubscriptionDetailData> GetSubscriptionDetailAsync(Guid subscriptionId, string iamUserId);

    /// <summary>
    /// Creates new service agreement consents with the given data for the given service
    /// </summary>
    /// <param name="subscriptionId">Id of the service subscription to create the consents for.</param>
    /// <param name="offerAgreementConsentData">service agreement consents</param>
    /// <param name="iamUserId">Id of the iam user</param>
    Task<Guid> CreateServiceAgreementConsentAsync(Guid subscriptionId, OfferAgreementConsentData offerAgreementConsentData,
        string iamUserId);

    /// <summary>
    /// Gets the service agreement data
    /// </summary>
    /// <param name="serviceId">Id of the service to get the agreements for</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetServiceAgreement(Guid serviceId);

    /// <summary>
    /// Gets the service consent detail data
    /// </summary>
    /// <param name="serviceConsentId">Id of the service consent</param>
    /// <returns>Returns the details</returns>
    Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId);

    /// <summary>
    /// Creates the non existing Consents for the given subscription id or updates the status of the existing
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="offerAgreementConsentData">Service Agreement Consent Data</param>
    /// <param name="iamUserId">id of the iam user</param>
    Task CreateOrUpdateServiceAgreementConsentAsync(Guid subscriptionId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId);

    /// <summary>
    /// Auto setup the service.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, string iamUserId);

    /// <summary>
    /// Updates the given service
    /// </summary>
    /// <param name="serviceId">Id of the service to update</param>
    /// <param name="data">Data of the updated entry</param>
    /// <param name="iamUserId">Id of the current user</param>
    Task UpdateServiceAsync(Guid serviceId, ServiceUpdateRequestData data, string iamUserId);

    /// <summary>
    /// Retrieves subscription statuses of provided services of the provided user's company.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="iamUserId">IAM ID of the user to retrieve app subscription statuses for.</param>
    /// <param name="sorting"></param>
    /// <param name="statusId"></param>
    /// <returns>Pagination of user's company's provided service' statuses.</returns>
    public Task<Pagination.Response<OfferCompanySubscriptionStatusData>> GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(int page, int size, string iamUserId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId);
}

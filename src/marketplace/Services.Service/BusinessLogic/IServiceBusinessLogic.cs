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
    /// Adds a subscription to the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the users company should be subscribed to</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <param name="identity">Identity of the user</param>
    /// <returns></returns>
    Task<Guid> AddServiceSubscription(Guid serviceId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Gets the service detail data for the given service
    /// </summary>
    /// <param name="serviceId">Id of the service the details should be retrieved for.</param>
    /// <param name="lang">Shortcode of the language for the text translations</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the service detail data</returns>
    Task<ServiceDetailResponse> GetServiceDetailsAsync(Guid serviceId, string lang, Guid companyId);

    /// <summary>
    /// Gets the Subscription Details for the given Id
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details for the subscription</returns>
    Task<SubscriptionDetailData> GetSubscriptionDetailAsync(Guid subscriptionId, Guid companyId);

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
    /// Auto setup the service.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="identity">Identity of the user</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Retrieves subscription statuses of provided services of the provided user's company.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="companyId"></param>
    /// <param name="sorting"></param>
    /// <param name="statusId"></param>
    /// <param name="offerId"></param>
    /// <returns>Pagination of user's company's provided service' statuses.</returns>
    Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(int page, int size, Guid companyId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId, Guid? offerId);

    /// <summary>
    /// Get the document content by given Id for Service
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="documentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(byte[] Content, string ContentType, string FileName)> GetServiceDocumentContentAsync(Guid serviceId, Guid documentId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all in review status offer in the marketplace.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="companyId"></param>
    /// <param name="sorting"></param>
    /// <param name="offerName"></param>
    /// <param name="statusId"></param>
    Task<Pagination.Response<AllOfferStatusData>> GetCompanyProvidedServiceStatusDataAsync(int page, int size, Guid companyId, OfferSorting? sorting, string? offerName, ServiceStatusIdFilter? statusId);

    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="serviceId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<ProviderSubscriptionDetailData> GetSubscriptionDetailForProvider(Guid serviceId, Guid subscriptionId, Guid companyId);

    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="serviceId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber(Guid serviceId, Guid subscriptionId, Guid companyId);

    /// <summary>
    /// Retrieves subscription statuses of subscribed Service of the provided user's company.
    /// </summary>
    /// <param name="companyId">Id of the users company to retrieve Service subscription statuses for.</param>
    /// <param name ="page">page</param>
    /// <param name ="size">size</param>
    /// <returns>Returns the details of the subscription status for Service user</returns>
    Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(int page, int size, Guid companyId);

    /// <summary>
    /// Starts the auto setup process.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns the response data</returns>
    Task StartAutoSetupAsync(OfferAutoSetupData data, Guid companyId);

    /// <summary>
    /// Unsubscribes an Service for the current users company.
    /// </summary>
    /// <param name="subscriptionId">ID of the subscription to unsubscribe from.</param>
    /// <param name="companyId">Id of the users company that initiated app unsubscription.</param>
    public Task UnsubscribeOwnCompanyServiceSubscriptionAsync(Guid subscriptionId, Guid companyId);
}

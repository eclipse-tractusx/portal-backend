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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Business logic for handling service release-related operations. Includes persistence layer access.
/// </summary>
public interface IServiceReleaseBusinessLogic
{
    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync();

    /// <summary>
    /// Retrieve Service Details by Id
    /// </summary>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    Task<ServiceData> GetServiceDetailsByIdAsync(Guid serviceId);

    /// <summary>
    /// Retrieve Service Type Data
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync();

    /// <summary>
    /// Retrieve Offer Agreemnet Consent Status Data
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetServiceAgreementConsentAsync(Guid serviceId, Guid companyId);

    /// <summary>
    /// Return Offer with Consent Status
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task<ServiceProviderResponse> GetServiceDetailsForStatusAsync(Guid serviceId, Guid companyId);

    /// <summary>
    /// Inserts or updates the consent to the specific service
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="offerAgreementConsents">Data of the consents for the agreements</param>
    /// <param name="companyId">Id of the users company</param>
    Task<IEnumerable<ConsentStatusData>> SubmitOfferConsentAsync(Guid serviceId, OfferAgreementConsent offerAgreementConsents, Guid companyId);

    /// <summary>
    /// Retrieves all in review status offer in the marketplace.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="sorting"></param>
    /// <param name="serviceName"></param>
    /// <param name="languageShortName"></param>
    /// <param name="statusId"></param>
    Task<Pagination.Response<InReviewServiceData>> GetAllInReviewStatusServiceAsync(int page, int size, OfferSorting? sorting, string? serviceName, string? languageShortName, ServiceReleaseStatusIdFilter? statusId);

    /// <summary>
    /// Creates a new service offering
    /// </summary>
    /// <param name="data">The data to create the service offering</param>
    /// <param name="identity">the identity</param>
    /// <returns>The id of the newly created service</returns>
    Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Updates the given service
    /// </summary>
    /// <param name="serviceId">Id of the service to update</param>
    /// <param name="data">Data of the updated entry</param>
    /// <param name="companyId">Id of the current users company</param>
    Task UpdateServiceAsync(Guid serviceId, ServiceUpdateRequestData data, Guid companyId);

    /// <summary>
    /// Update app status and create notification
    /// </summary>
    /// <param name="serviceId">Id of the service that should be submitted</param>
    /// <param name="userId">Id of the User</param>
    Task SubmitServiceAsync(Guid serviceId, Guid userId);

    /// <summary>
    /// Approve Service Status from IN_Review to Active
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task ApproveServiceRequestAsync(Guid appId, Guid userId);

    /// <summary>
    /// Declines the service request
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="userId">Id of the User</param>
    /// <param name="data">The decline request data</param>
    Task DeclineServiceRequestAsync(Guid serviceId, Guid userId, OfferDeclineRequest data);

    /// <summary>
    /// Upload document for given company user for Service
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="identity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateServiceDocumentAsync(Guid serviceId, DocumentTypeId documentTypeId, IFormFile document, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken);

    /// <summary>
    /// Delete the Service Document
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task DeleteServiceDocumentsAsync(Guid documentId, Guid companyId);

    /// <summary>
    /// Get technical user profiles for a specific offer
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>AsyncEnumerable with the technical user profile information</returns>
    Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, Guid companyId);

    /// <summary>
    /// Creates or updates the technical user profiles
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="data">The technical user profiles</param>
    /// <param name="companyId">Id of the users company</param>
    Task UpdateTechnicalUserProfiles(Guid serviceId, IEnumerable<TechnicalUserProfileData> data, Guid companyId);
}

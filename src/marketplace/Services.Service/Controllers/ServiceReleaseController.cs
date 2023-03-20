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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating services.
/// </summary>
[Route("api/services/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceReleaseController : ControllerBase
{
    private readonly IServiceReleaseBusinessLogic _serviceReleaseBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceReleaseBusinessLogic">Logic dependency.</param>
    public ServiceReleaseController(IServiceReleaseBusinessLogic serviceReleaseBusinessLogic)
    {
        _serviceReleaseBusinessLogic = serviceReleaseBusinessLogic;
    }

    /// <summary>
    /// Return Agreement Data for offer_type_id Service
    /// </summary>
    /// <remarks>Example: GET: /api/services/servicerelease/agreementData</remarks>
    /// <response code="200">Returns the Cpllection of agreement data</response>
    [HttpGet]
    [Route("agreementData")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AgreementDocumentData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync() =>
        _serviceReleaseBusinessLogic.GetServiceAgreementDataAsync();

    /// <summary>
    /// Retrieves service details for an offer referenced by id.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the offer to retrieve.</param>
    /// <returns>ServiceData for requested offer.</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/inReview/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested service details.</response>
    /// <response code="404">service not found.</response>
    /// <response code="409">service is inCorrect Status.</response>
    [HttpGet]
    [Route("inReview/{serviceId}", Name = nameof(GetServiceDetailsByIdAsync))]
    [Authorize(Roles = "view_service_details")]
    [ProducesResponseType(typeof(ServiceData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<ServiceData> GetServiceDetailsByIdAsync([FromRoute] Guid serviceId) =>
        _serviceReleaseBusinessLogic.GetServiceDetailsByIdAsync(serviceId);

    /// <summary>
    /// Retrieve Service Type Data
    /// </summary>
    /// <returns>Service Type Data</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/serviceTypes </remarks>
    /// <response code="200">Returns the Service Type.</response>
    [HttpGet]
    [Route("serviceTypes")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ServiceTypeData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync() =>
        _serviceReleaseBusinessLogic.GetServiceTypeDataAsync();
    
    /// <summary>
    /// Gets the agreement consent status for the given service id
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: GET: /api/services/servicerelease/consent/{serviceId}</remarks>
    /// <response code="200">Returns the offer Agreement Consent data</response>
    /// <response code="404">offer does not exist.</response>
    /// <response code="403">User not associated with offer.</response>
    [HttpGet]
    [Route("consent/{serviceId}")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(OfferAgreementConsent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<OfferAgreementConsent> GetServiceAgreementConsentByIdAsync([FromRoute] Guid serviceId) =>
       this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.GetServiceAgreementConsentAsync(serviceId, iamUserId));

    /// <summary>
    /// Return app detail with status
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: GET: /api/services/servicerelease/{serviceId}/serviceStatus</remarks>
    /// <response code="200">Return the Offer and status data</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="403">User not associated with provider company.</response>
    [HttpGet]
    [Route("{serviceId}/serviceStatus")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(OfferProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<OfferProviderResponse> GetServiceDetailsForStatusAsync([FromRoute] Guid serviceId) =>
        this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.GetServiceDetailsForStatusAsync(serviceId, iamUserId));
}

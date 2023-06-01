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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="SubscriptionConfigurationController"/>
/// </summary>
[ApiController]
[Route("api/administration/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class SubscriptionConfigurationController : ControllerBase
{
    private readonly ISubscriptionConfigurationBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="businessLogic">Prozess business logic.</param>
    public SubscriptionConfigurationController(ISubscriptionConfigurationBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// get detail data of the calling users service provider
    /// </summary>
    /// <returns></returns>
    /// <remarks>Example: GET: api/administration/subscriptionconfiguration/owncompany</remarks>
    /// <response code="200">The service provider details.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="403">The calling users company is not a service-provider</response>
    /// <response code="409">User is not assigned to company.</response>
    [HttpGet]
    [Route("owncompany", Name = nameof(GetServiceProviderCompanyDetail))]
    [Authorize(Roles = "add_service_offering, add_apps")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(ProviderDetailReturnData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<ProviderDetailReturnData> GetServiceProviderCompanyDetail() =>
        this.WithCompanyId(companyId => _businessLogic.GetProviderCompanyDetailsAsync(companyId));

    /// <summary>
    /// Sets detail data to the calling users service provider
    /// </summary>
    /// <param name="data">Service provider detail data</param>
    /// <returns></returns>
    /// <remarks>Example: PUT: api/administration/subscriptionconfiguration/owncompany</remarks>
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpPut]
    [Route("owncompany")]
    [Authorize(Roles = "add_service_offering, add_apps")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> SetProviderCompanyDetail([FromBody] ProviderDetailData data)
    {
        await this.WithCompanyId(companyId => _businessLogic.SetProviderCompanyDetailsAsync(data, companyId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Gets the process steps for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/subscriptionconfiguration/offer-subscription/{offerSubscriptionId}
    /// <response code="200">Returns the process steps.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpGet]
    [Authorize(Roles = "view_autosetup_status")]
    [Route("process/offer-subscription/{offerSubscriptionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IAsyncEnumerable<ProcessStepData> GetProcessStepsForSubscription([FromRoute] Guid offerSubscriptionId) =>
        _businessLogic.GetProcessStepsForSubscription(offerSubscriptionId);

    /// <summary>
    /// Retriggers the provider for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/subscriptionconfiguration/offer-subscription/{offerSubscriptionId}/retrigger-provider
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "retrigger_subscription, activate_subscription")]
    [Route("process/offer-subscription/{offerSubscriptionId}/retrigger-provider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerProvider([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.RetriggerProvider(offerSubscriptionId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the client creation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/subscriptionconfiguration/offer-subscription/{offerSubscriptionId}/retrigger-create-client
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "retrigger_subscription")]
    [Route("process/offer-subscription/{offerSubscriptionId}/retrigger-create-client")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCreateClient([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.RetriggerCreateClient(offerSubscriptionId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the technical user creation for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/subscriptionconfiguration/offer-subscription/{offerSubscriptionId}/retrigger-create-technical-user
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status PENDING or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "retrigger_subscriptionn")]
    [Route("process/offer-subscription/{offerSubscriptionId}/retrigger-create-technical-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCreateTechnicalUser([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.RetriggerCreateTechnicalUser(offerSubscriptionId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the provider callback for the given offer subscription id
    /// </summary>
    /// <param name="offerSubscriptionId" example="22dbc488-8f90-40b4-9fbd-ea0b246e827b">Id of the offer subscription that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/subscriptionconfiguration/offer-subscription/{offerSubscriptionId}/retrigger-provider-callback
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the OfferSubscription is not in status ACTIVE or the next step can't automatically retriggered.</response>
    /// <response code="404">No OfferSubscription found for the offerSubscriptionId.</response>
    [HttpPost]
    [Authorize(Roles = "retrigger_subscription, activate_subscription")]
    [Route("process/offer-subscription/{offerSubscriptionId}/retrigger-provider-callback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerProviderCallback([FromRoute] Guid offerSubscriptionId)
    {
        await _businessLogic.RetriggerProviderCallback(offerSubscriptionId).ConfigureAwait(false);
        return NoContent();
    }
}

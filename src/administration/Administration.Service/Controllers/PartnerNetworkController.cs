/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="PartnerNetworkController"/>
/// </summary>
[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class PartnerNetworkController : ControllerBase
{
    private readonly IPartnerNetworkBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="PartnerNetworkController"/>
    /// </summary>
    /// <param name="logic">The partner network business logic</param>
    public PartnerNetworkController(IPartnerNetworkBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary> Get all member companies</summary>
    /// <param name="bpnIds">BPN Id's</param>
    /// <returns>Returns all the active member companies bpn.</returns>
    /// <remarks>Example: GET: api/administration/partnernetwork/memberCompanies</remarks>
    /// <response code="200">Returns all the active member companies bpn.</response>

    [HttpGet]
    [Authorize(Roles = "view_membership")]
    [Route("memberCompanies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [PublicUrl(CompanyRoleId.ACTIVE_PARTICIPANT, CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.APP_PROVIDER)]
    public IAsyncEnumerable<string> GetAllMemberCompaniesBPNAsync([FromQuery] IEnumerable<string>? bpnIds = null) =>
        _logic.GetAllMemberCompaniesBPNAsync(bpnIds);

    /// <summary>
    /// Gets partner network data from BPN Pool
    /// </summary>
    /// <param name="page" example="0">The page of partner network data, default is 0.</param>
    /// <param name="size" example="10">Amount of partner network data, default is 10.</param>
    /// <param name="partnerNetworkRequest">The bpnls to get the selected record</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a List of partner networks</returns>
    /// <remarks>Example: Get: /api/registration/legalEntities/search?page=0&amp;size=10&amp;bpnl=</remarks>
    /// <response code="200">Returns the list of partner networks</response>
    /// <response code="503">The requested service responded with the given error.</response>
    [HttpPost]
    [Authorize(Roles = "view_partner_network")]
    [Route("legalEntities/search")]
    [ProducesResponseType(typeof(PartnerNetworkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public Task<PartnerNetworkResponse> GetPartnerNetworkDataAsync([FromBody] PartnerNetworkRequest partnerNetworkRequest, [FromQuery] int page = 0, [FromQuery] int size = 10, CancellationToken cancellationToken = default) =>
        this.WithBearerToken(token => _logic.GetPartnerNetworkDataAsync(page, size, partnerNetworkRequest, token, cancellationToken));
}

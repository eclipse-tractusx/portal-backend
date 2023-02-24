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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="PartnerNetworkController"/>
/// </summary>
[ApiController]
[Route("api/administration/partnernetwork")]
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
    /// <returns>Returns all the active member companies bpn.</returns>
    /// <remarks>Example: GET: api/administration/partnernetwork/memberCompanies</remarks>
    /// <response code="200">Returns all the active member companies bpn.</response>

    [HttpGet]
    [Authorize(Roles = "view_membership")]
    [Route("memberCompanies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync() =>
        _logic.GetAllMemberCompaniesBPNAsync();
}

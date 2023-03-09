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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers;

/// <summary>
/// Controller providing actions for updating applications.
/// </summary>
[Route("api/apps/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class AppChangeController : ControllerBase
{
    private readonly IAppChangeBusinessLogic _businessLogic;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="businessLogic"></param>
    public AppChangeController(IAppChangeBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }
    
    /// <summary>
    /// dd role and role description for Active App 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userRoles"></param>
    /// <remarks>Example: POST: /api/apps/appchange/{appId}/role/activeapp</remarks>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="200">created role and role description successfully.</response>
    /// <response code="403">User not associated with provider company.</response>
    /// <response code="409">App provider company not set.</response>
    [HttpPost]
    [Route("{appId}/role/activeapp")]
    [Authorize(Roles = "edit_apps")]
    [ProducesResponseType(typeof(IEnumerable<AppRoleData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IEnumerable<AppRoleData>> AddActiveAppUserRole([FromRoute] Guid appId, [FromBody] IEnumerable<AppUserRole> userRoles)=>
        await this.WithIamUserId(iamUserId => _businessLogic.AddActiveAppUserRoleAsync(appId, userRoles, iamUserId)).ConfigureAwait(false);
}

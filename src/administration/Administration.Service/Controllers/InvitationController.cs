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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions execute invitation
/// </summary>
[ApiController]
[Route("api/administration/invitation")]
[Produces("application/json")]
[Consumes("application/json")]
public class InvitationController : ControllerBase
{
	private readonly IInvitationBusinessLogic _logic;

	/// <summary>
	/// Creates a new instance of <see cref="InvitationController"/>
	/// </summary>
	/// <param name="logic">The invitation business logic</param>
	public InvitationController(IInvitationBusinessLogic logic)
	{
		_logic = logic;
	}

	/// <summary>
	/// Executes the invitation
	/// </summary>
	/// <param name="invitationData"></param>
	/// <returns></returns>
	/// <remarks>
	/// Example: POST: api/administration/invitation
	/// </remarks>
	/// <response code="200">Successfully executed the invitation.</response>
	/// <response code="400">Missing mandatory input values (e.g. email, organization name, etc.)</response>
	/// <response code="500">Internal Server Error.</response>
	/// <response code="502">Bad Gateway Service Error.</response>
	/// <response code="409">user is not associated with  company.</response>
	[HttpPost]
	[Authorize(Roles = "invite_new_partner")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
	public Task ExecuteInvitation([FromBody] CompanyInvitationData invitationData) =>
		this.WithIamUserId(iamUserId => _logic.ExecuteInvitation(invitationData, iamUserId));
}

/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for companies.
/// </summary>
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "[controller]")]
[ApiController]
[Produces("application/json")]
public class MailController : ControllerBase
{
    private readonly IMailBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="businessLogic">Mail business logic.</param>
    public MailController(IMailBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Creates a mail from the given data
    /// </summary>
    /// <param name="data">Data for the mail</param>
    /// <returns>Return NoContent</returns>
    /// <remarks>Example: POST: /api/administration/mail/ssi-credentials</remarks>
    [HttpPost]
    [Route("ssi-credentials")]
    [Authorize(Roles = "send_mail")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(int), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SendMail([FromBody] MailData data)
    {
        await _businessLogic.SendMail(data).ConfigureAwait(false);
        return NoContent();
    }
}

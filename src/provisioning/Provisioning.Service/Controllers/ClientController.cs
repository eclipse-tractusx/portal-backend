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

using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Service.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Service.Controllers
{
    /// <summary>
    /// The controller provides the possibility to create a client
    /// </summary>
    [ApiController]
    [Route("api/provisioning")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ClientController : ControllerBase
    {

        private readonly ILogger<ClientController> _logger;
        private readonly IClientBusinessLogic _logic;

        /// <summary>
        /// Creates a instance of <see cref="ClientController"/>
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="logic">Client business logic</param>
        public ClientController(ILogger<ClientController> logger, IClientBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        /// <summary>
        /// Creates a client with the given data
        /// </summary>
        /// <param name="clientSetupData">the setup data for the new client</param>
        /// <returns>Returns the client id of the created client</returns>
        /// <remarks>Example: Get: /api/provisioning/client/setup</remarks>
        /// <response code="200">Successfully created the client.</response>
        [HttpPost]
        [Authorize(Roles="setup_client")]
        [Route("client/setup")]
        [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateClient([FromBody] ClientSetupData clientSetupData)
        {
            try
            {
                var clientId = await _logic.CreateClient(clientSetupData).ConfigureAwait(false);
                return new OkObjectResult(clientId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}

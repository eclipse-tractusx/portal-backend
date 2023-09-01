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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos;

[ApiController]
[Route("api/info")]
public class OpenInformationController : ControllerBase
{
    private readonly IPublicInformationBusinessLogic _publicInformationBusinessLogic;

    /// <summary>
    /// Creates a new instance of <see cref="OpenInformationController"/>
    /// </summary>
    /// <param name="publicInformationBusinessLogic">The business logic</param>
    public OpenInformationController(IPublicInformationBusinessLogic publicInformationBusinessLogic)
    {
        _publicInformationBusinessLogic = publicInformationBusinessLogic;
    }

    /// <summary>
    /// Gets all open information based on the 
    /// </summary>
    /// <remarks>
    /// Example: GET: api/info
    /// </remarks>
    /// <response code="200">Successfully executed the invitation.</response>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(IEnumerable<UrlInformation>), StatusCodes.Status200OK)]
    public Task<IEnumerable<UrlInformation>> GetOpenUrls() =>
        _publicInformationBusinessLogic.GetPublicUrls();
}

/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Controller;

[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "errormessage")]
[Produces("application/json")]
public class ErrorMessageController : ControllerBase
{
    private readonly IErrorMessageService _logic;

    public ErrorMessageController(IErrorMessageService logic)
    {
        _logic = logic;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IErrorMessageService.ErrorMessageType>), StatusCodes.Status200OK)]
    public IEnumerable<IErrorMessageService.ErrorMessageType> GetAllMessageTexts()
        => _logic.GetAllMessageTexts();
}

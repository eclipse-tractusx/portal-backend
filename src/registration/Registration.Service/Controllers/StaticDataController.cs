/********************************************************************************
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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers
{

    [ApiController]
    [EnvironmentRoute("MVC_ROUTING_BASEPATH", "[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class StaticDataController : ControllerBase
    {
        private readonly IStaticDataBusinessLogic _staticDataBusinessLogic;

        /// <summary>
        /// Creates a new instance of <see cref="StaticDataController"/>
        /// </summary>
        /// <param name="staticDataBusinessLogic">Access to the business logic</param>
        public StaticDataController(IStaticDataBusinessLogic staticDataBusinessLogic)
        {
            _staticDataBusinessLogic = staticDataBusinessLogic;
        }

        /// <summary>
        /// Retrieve all app countries - short name (2digit) and countries long name
        /// </summary>
        /// <returns>AsyncEnumerable of Countries Long name Data</returns>
        /// <remarks>
        /// Example: GET: /api/registration/staticdata/countrylist
        /// </remarks> 
        /// <response code="200">Returns a list of all countries long name with language code i.e german and english</response>
        [HttpGet]
        [Route("countrylist")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CountryLongNameData>), StatusCodes.Status200OK)]
        public IAsyncEnumerable<CountryLongNameData> GetCountries() =>
            _staticDataBusinessLogic.GetAllCountries();
    }
}

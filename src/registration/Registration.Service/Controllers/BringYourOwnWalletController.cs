/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers
{
    [ApiController]
    [EnvironmentRoute("MVC_ROUTING_BASEPATH", "bringYourOwnWallet")]
    public class BringYourOwnWalletController(IBringYourOwnWalletBusinessLogic logic) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("{did}/validateDid")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<NoContentResult> validateDid([FromRoute] string did, CancellationToken cancellationToken)
        {

            await logic.ValidateDid(did, cancellationToken);

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("{companyId}/saveHolderDid/{did}")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<NoContentResult> SaveHolderDid([FromRoute] Guid companyId, [FromRoute] string did)
        {
            await logic.SaveCustomerWalletAsync(companyId, did);

            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "submit_registration")]
        [Route("{companyId}/getHolderDid")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<string> GetHolderDid([FromRoute] Guid companyId)
        {
            return await logic.getCompanyWalletDidAsync(companyId);
        }
    }
}

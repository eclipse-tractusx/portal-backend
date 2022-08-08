/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating identityProviders for companies.
/// </summary>
[Route("api/administration/identityprovider")]
[ApiController]
public class IdentityProviderController : ControllerBase
{
    private readonly IIdentityProviderBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="identityProviderBusinessLogic">IdentityProvider business logic.</param>
    public IdentityProviderController(IIdentityProviderBusinessLogic identityProviderBusinessLogic)
    {
        _businessLogic = identityProviderBusinessLogic;
    }

    [HttpGet]
    [Authorize(Roles = "view_idp")]
    [Route("owncompany/identityproviders")]
    public ValueTask<List<IdentityProviderDetails>> GetOwnCompanyIdentityProviderDetails() =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProvidersAsync(iamUserId).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "add_idp")]
    [Route("owncompany/identityproviders")]
    public async ValueTask<ActionResult<IdentityProviderDetails>> CreateOwnCompanyIdentityProvider([FromQuery]IamIdentityProviderProtocol protocol)
    {
        var details = await this.WithIamUserId(iamUserId => _businessLogic.CreateOwnCompanyIdentityProviderAsync(protocol, iamUserId)).ConfigureAwait(false);
        return (ActionResult<IdentityProviderDetails>) CreatedAtRoute(nameof(GetOwnCompanyIdentityProvider), new { identityProviderId = details.identityProviderId }, details );
    }

    [HttpGet]
    [Authorize(Roles = "view_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}", Name=nameof(GetOwnCompanyIdentityProvider))]
    public ValueTask<IdentityProviderDetails> GetOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProviderAsync(identityProviderId, iamUserId));

    [HttpPut]
    [Authorize(Roles = "setup_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public ValueTask<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId, [FromBody] IdentityProviderEditableDetails details) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, details, iamUserId));

    [HttpDelete]
    [Authorize(Roles = "delete_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public async Task<ActionResult> DeleteOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId)
    {
        await this.WithIamUserId(iamUserId => _businessLogic.DeleteOwnCompanyIdentityProviderAsync(identityProviderId, iamUserId)).ConfigureAwait(false);
        return (ActionResult) NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Route("owncompany/users")]
    public IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync([FromQuery] IEnumerable<Guid> identityProviderIds, [FromQuery]bool unlinkedUsersOnly = false) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, iamUserId, unlinkedUsersOnly));

    [HttpGet]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/usersfile")]
    public IActionResult GetOwnCompanyUsersIdentityProviderFileAsync([FromQuery] IEnumerable<Guid> identityProviderIds, [FromQuery]bool unlinkedUsersOnly = false) {
        var (stream, contentType, fileName, encoding) = this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUsersIdentityProviderLinkDataStream(identityProviderIds, iamUserId, unlinkedUsersOnly));
        return File(stream, string.Join("; ", contentType, encoding.WebName), fileName);
    }

    [HttpPost]
    [Authorize(Roles = "modify_user_account")]
    [Consumes("multipart/form-data")]
    [Route("owncompany/usersfile")]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    [ProducesResponseType(typeof(IdentityProviderUpdateStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
        public ValueTask<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderFileAsync([FromForm(Name = "document")] IFormFile document) =>
            this.WithIamUserId(iamUserId => _businessLogic.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(document, iamUserId));


    [HttpPost]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider")]
    public async ValueTask<ActionResult<UserIdentityProviderLinkData>> AddOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromBody] UserIdentityProviderLinkData identityProviderLinkData)
    {
        var linkData = await this.WithIamUserId(iamUserId => _businessLogic.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderLinkData, iamUserId)).ConfigureAwait(false);
        return (ActionResult<UserIdentityProviderLinkData>) CreatedAtRoute(
            nameof(GetOwnCompanyUserIdentityProviderDataAsync),
            new { companyUserId = companyUserId, identityProviderId = linkData.identityProviderId },
            linkData);
    }

    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public ValueTask<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId, [FromBody] UserLinkData userLinkData) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, userLinkData, iamUserId));

    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}", Name = nameof(GetOwnCompanyUserIdentityProviderDataAsync))]
    public ValueTask<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, iamUserId));

    [HttpDelete]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public async ValueTask<ActionResult> DeleteOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId)
    {
        await this.WithIamUserId(iamUserId => _businessLogic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId, iamUserId)).ConfigureAwait(false);
        return (ActionResult) NoContent();
    }
}

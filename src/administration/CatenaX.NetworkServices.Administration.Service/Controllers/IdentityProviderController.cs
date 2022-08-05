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
    public Task<IEnumerable<IdentityProviderDetails>> GetOwnCompanyIdentityProviderDetails() =>
        this.WithIamUserId(async iamUserId =>
            (await _businessLogic.GetOwnCompanyIdentityProviders(iamUserId).ToListAsync().ConfigureAwait(false))
            .AsEnumerable());

    [HttpPost]
    [Authorize(Roles = "add_idp")]
    [Route("owncompany/identityproviders")]
    public Task<ActionResult<IdentityProviderDetails>> CreateOwnCompanyIdentityProvider([FromQuery]IamIdentityProviderProtocol protocol) =>
        this.WithIamUserId(async iamUserId =>
        {
            var details = await _businessLogic.CreateOwnCompanyIdentityProvider(protocol, iamUserId).ConfigureAwait(false);
            return (ActionResult<IdentityProviderDetails>) CreatedAtRoute(nameof(GetOwnCompanyIdentityProvider), new { identityProviderId = details.identityProviderId }, details );
        });

    [HttpGet]
    [Authorize(Roles = "view_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}", Name=nameof(GetOwnCompanyIdentityProvider))]
    public Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProvider(identityProviderId, iamUserId));

    [HttpPut]
    [Authorize(Roles = "setup_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId, [FromBody] IdentityProviderEditableDetails details) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyIdentityProvider(identityProviderId, details, iamUserId));

    [HttpDelete]
    [Authorize(Roles = "delete_idp")]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<ActionResult> DeleteOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(async iamUserId =>
        {
            await _businessLogic.DeleteOwnCompanyIdentityProvider(identityProviderId, iamUserId).ConfigureAwait(false);
            return (ActionResult) NoContent();
        });

    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Route("owncompany/users")]
    public IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync([FromQuery] IEnumerable<Guid> identityProviderIds) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, iamUserId));

    [HttpGet]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/usersfile")]
    public IActionResult GetOwnCompanyUsersIdentityProviderFileAsync([FromQuery] IEnumerable<Guid> identityProviderIds) {
        var (stream, contentType, fileName, encoding) = this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUsersIdentityProviderLinkDataStream(identityProviderIds, iamUserId));
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
        public Task<IdentityProviderUpdateStats> UploadOwnCompanyUsersIdentityProviderFileAsync([FromForm(Name = "document")] IFormFile document) =>
            this.WithIamUserId(iamUserId => _businessLogic.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(document, iamUserId));


    [HttpPost]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider")]
    public Task<ActionResult<UserIdentityProviderLinkData>> AddOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromBody] UserIdentityProviderLinkData identityProviderLinkData) =>
        this.WithIamUserId(async iamUserId => 
        {
            var linkData = await _businessLogic.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderLinkData, iamUserId).ConfigureAwait(false);
            return (ActionResult<UserIdentityProviderLinkData>) CreatedAtRoute(
                nameof(GetOwnCompanyUserIdentityProviderDataAsync),
                new { companyUserId = companyUserId, identityProviderId = linkData.identityProviderId },
                linkData);
        });

    [HttpPut]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public Task<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId, [FromBody] UserLinkData userLinkData) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, userLinkData, iamUserId));

    [HttpGet]
    [Authorize(Roles = "view_user_management")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}", Name = nameof(GetOwnCompanyUserIdentityProviderDataAsync))]
    public Task<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, iamUserId));

    [HttpDelete]
    [Authorize(Roles = "modify_user_account")]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public Task<ActionResult> DeleteOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(async iamUserId =>
        {
            await _businessLogic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
            return (ActionResult) NoContent();
        });
}

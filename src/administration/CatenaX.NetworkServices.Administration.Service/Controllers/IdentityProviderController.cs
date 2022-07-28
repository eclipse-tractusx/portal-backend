using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
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
    [Route("owncompany/identityproviders")]
    public Task<IEnumerable<IdentityProviderDetails>> GetOwnCompanyIdentityProviderDetails() =>
        this.WithIamUserId(async iamUserId =>
            (await _businessLogic.GetOwnCompanyIdentityProviders(iamUserId).ToListAsync().ConfigureAwait(false))
            .AsEnumerable());

    [HttpPost]
    [Route("owncompany/identityproviders")]
    public Task<ActionResult<IdentityProviderDetails>> CreateOwnCompanyIdentityProvider([FromQuery]IamIdentityProviderProtocol protocol) =>
        this.WithIamUserId(async iamUserId =>
        {
            var details = await _businessLogic.CreateOwnCompanyIdentityProvider(protocol, iamUserId).ConfigureAwait(false);
            return (ActionResult<IdentityProviderDetails>) CreatedAtRoute(nameof(GetOwnCompanyIdentityProvider), new { identityProviderId = details.identityProviderId }, details );
        });

    [HttpGet]
    [Route("owncompany/identityproviders/{identityProviderId}", Name=nameof(GetOwnCompanyIdentityProvider))]
    public Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProvider(identityProviderId, iamUserId));

    [HttpPut]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId, [FromBody] IdentityProviderEditableDetails details) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyIdentityProvider(identityProviderId, details, iamUserId));

    [HttpDelete]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<ActionResult> DeleteOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(async iamUserId =>
        {
            await _businessLogic.DeleteOwnCompanyIdentityProvider(identityProviderId, iamUserId).ConfigureAwait(false);
            return (ActionResult) NoContent();
        });

    [HttpGet]
    [Route("owncompany/users")]
    public IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync([FromQuery] IEnumerable<Guid> identityProviderIds) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUsersIdentityProviderDataAsync(identityProviderIds, iamUserId));

    [HttpPost]
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
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public Task<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId, [FromBody] UserLinkData userLinkData) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, userLinkData, iamUserId));

    [HttpGet]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}", Name = nameof(GetOwnCompanyUserIdentityProviderDataAsync))]
    public Task<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, iamUserId));

    [HttpDelete]
    [Route("owncompany/users/{companyUserId}/identityprovider/{identityProviderId}")]
    public Task<ActionResult> DeleteOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(async iamUserId =>
        {
            await _businessLogic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId, iamUserId).ConfigureAwait(false);
            return (ActionResult) NoContent();
        });
}

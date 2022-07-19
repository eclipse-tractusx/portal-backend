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
    public IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviderDetails() =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProviders(iamUserId));

    [HttpPut]
    [Route("owncompany/identityproviders")]
    public Task<IdentityProviderDetails> CreateOwnCompanyIdentityProvider([FromQuery]IamIdentityProviderProtocol protocol) =>
        this.WithIamUserId(iamUserId => _businessLogic.CreateOwnCompanyIdentityProvider(protocol, iamUserId));

    [HttpGet]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProvider(identityProviderId, iamUserId));

    [HttpPost]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId, [FromBody] IdentityProviderEditableDetails details) =>
        this.WithIamUserId(iamUserId => _businessLogic.UpdateOwnCompanyIdentityProvider(identityProviderId, details, iamUserId));

    [HttpDelete]
    [Route("owncompany/identityproviders/{identityProviderId}")]
    public Task DeleteOwnCompanyIdentityProvider([FromRoute] Guid identityProviderId) =>
        this.WithIamUserId(iamUserId => _businessLogic.DeleteOwnCompanyIdentityProvider(identityProviderId, iamUserId));

    [HttpGet]
    [Route("owncompany/users")]
    public IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUserIdentityProviderDataAsync([FromQuery] IEnumerable<string> aliase) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyUserIdentityProviderDataAsync(aliase, iamUserId));

    [HttpPut]
    [Route("owncompany/users/{companyUserId}/identityprovider/{alias}")]
    public Task<UserIdentityProviderData> AddOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] string alias, [FromBody] UserLinkData userLinkData) =>
        this.WithIamUserId(iamUserId => _businessLogic.CreateOwnCompanyUserIdentityProviderDataAsync(companyUserId, alias, userLinkData, iamUserId));

    [HttpDelete]
    [Route("owncompany/users/{companyUserId}/identityprovider/{alias}")]
    public Task<UserIdentityProviderData> DeleteOwnCompanyUserIdentityProviderDataAsync([FromRoute] Guid companyUserId, [FromRoute] string alias) =>
        this.WithIamUserId(iamUserId => _businessLogic.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, alias, iamUserId));
}

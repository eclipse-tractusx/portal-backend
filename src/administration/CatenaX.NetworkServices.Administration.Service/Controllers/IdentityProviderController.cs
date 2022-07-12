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
}

using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating identityProviders for companies.
/// </summary>
[Route("api/administration/[controller]")]
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
    [Route("")]
    public IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviderDetails() =>
        this.WithIamUserId(iamUserId => _businessLogic.GetOwnCompanyIdentityProviders(iamUserId));
}

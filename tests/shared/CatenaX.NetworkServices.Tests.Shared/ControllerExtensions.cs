using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Tests.Shared;

/// <summary>
/// Extension methods for the controller
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Creates a claum for the identity user and adds it to the controller context
    /// </summary>
    /// <param name="controller">The controller that should be enriched</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    public static void AddControllerContextWithClaim(this ControllerBase controller, string iamUserId)
    {
        var claimsIdentity = new ClaimsIdentity();
        claimsIdentity.AddClaims(new[] {new Claim("sub", iamUserId)});
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(claimsIdentity)
        };

        var controllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.ControllerContext = controllerContext;
    }
}
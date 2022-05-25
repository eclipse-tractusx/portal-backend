using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Keycloak.Authentication;

/// <summary>
/// Extension methods for API controllers.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Determines IamUserId from request claims and subsequently calls a provided function with iamUserId as parameter.
    /// </summary>
    /// <typeparam name="T">Return type of the controller function.</typeparam>
    /// <param name="controller">Controller to extend.</param>
    /// <param name="idConsumingFunction">Function that is called with iamUserId parameter.</param>
    /// <returns>Result of inner function.</returns>
    /// <exception cref="ArgumentException">If expected claim value is not provided.</exception>
    public static T WithIamUserId<T>(this ControllerBase controller, Func<string, T> idConsumingFunction)
    {
        var sub = controller.User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
        {
            throw new ArgumentException("Claim 'sub' must not be null or empty.", nameof(sub));
        }
        return idConsumingFunction(sub);
    }
}

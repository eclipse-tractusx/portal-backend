using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Keycloak.Authentication;

public static class GetUserControllerExtension
{
    public static T WithIamUserId<T>(this ControllerBase controller, Func<string, T> _next)
    {
        var sub = controller.User.Claims.SingleOrDefault(x => x.Type == "sub")?.Value as string;
        if (String.IsNullOrWhiteSpace(sub))
        {
            throw new ArgumentException("claim sub must not be null or empty","sub");
        }
        return _next(sub);
    }
}

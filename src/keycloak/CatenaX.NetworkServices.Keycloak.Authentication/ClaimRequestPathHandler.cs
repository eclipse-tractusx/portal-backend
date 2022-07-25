using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Keycloak.Authentication
{
    public class ClaimRequestPathRequirement : IAuthorizationRequirement
    {
        private readonly string _claim;
        private readonly string _parameter;
        public ClaimRequestPathRequirement(string claim, string parameter)
        {
            _claim = claim;
            _parameter = parameter;
        }
        public bool IsSuccess(IDictionary<string,object> routeValues, IEnumerable<Claim> claims)
        {
            var routeValue = routeValues[_parameter];
            if (routeValue == null) return false;
            var claim = claims.SingleOrDefault( x => x.Type == _claim );
            if (claim == null) return false;
            return claim.Value.Equals(routeValue);
        }
    }

    public class ClaimRequestPathHandler : AuthorizationHandler<ClaimRequestPathRequirement>
    {
        private IHttpContextAccessor _contextAccessor;

        public ClaimRequestPathHandler (IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequestPathRequirement requirement)
        {
            if (requirement.IsSuccess(_contextAccessor.HttpContext.Request.RouteValues, context.User.Claims))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}

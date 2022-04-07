using System.Json;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Keycloak.Authentication
{
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        readonly JwtBearerOptions _Options;
        public KeycloakClaimsTransformation(IOptions<JwtBearerOptions> options)
        {
            _Options = options.Value;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var resource_access = principal.Claims.FirstOrDefault(claim => claim.Type == "resource_access" && claim.ValueType == "JSON")?.Value;
            if (resource_access != null)
            {
                JsonValue audience = null;
                if((JsonValue.Parse(resource_access) as JsonObject)?.TryGetValue(_Options.TokenValidationParameters.ValidAudience, out audience) ?? false)
                {
                    JsonValue roles = null;
                    if (((audience as JsonObject)?.TryGetValue("roles", out roles) ?? false) && roles is JsonArray)
                    {
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                        bool rolesAdded = false;
                        foreach(JsonValue role in roles)
                        {
                            if (role.JsonType == JsonType.String)
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role,role));
                                rolesAdded = true;
                            }
                        }
                        if (rolesAdded)
                        {
                            principal.AddIdentity(claimsIdentity);
                        }
                    }
                }
            }
            return Task.FromResult(principal);
        }
    }
}

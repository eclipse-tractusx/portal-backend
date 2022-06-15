using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CatenaX.NetworkServices.Framework.Swagger;

public class AddAuthFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizeAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .Concat(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .ToList();

        if (!authorizeAttributes.Any()) return;

        
        var authorizationDescription = new StringBuilder(" (Authorization required");
        var policies = authorizeAttributes
            .Where(a => !string.IsNullOrEmpty(a.Roles))
            .Select(a => a.Roles)
            .OrderBy(role => role)
            .ToList();

        if (policies.Any())
        {
            authorizationDescription.Append($" - Roles: {string.Join(", ", policies)};");
        }
        
        operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = "The User is unauthorized" });
        operation.Summary += authorizationDescription.ToString().TrimEnd(';') + ")";
    }
}
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CatenaX.NetworkServices.Framework.Swagger;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizationRequired = context.MethodInfo
            .GetCustomAttribute(typeof(AuthorizeAttribute), true) != null;

        if (authorizationRequired)
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "The User is unauthorized" });
        }
    }
}

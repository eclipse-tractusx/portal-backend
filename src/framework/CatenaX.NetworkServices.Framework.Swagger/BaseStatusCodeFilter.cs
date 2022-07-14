using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CatenaX.NetworkServices.Framework.Swagger;

public class BaseStatusCodeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var statusCode500 = StatusCodes.Status500InternalServerError.ToString();
        if (!operation.Responses.ContainsKey(statusCode500))
        {
            operation.Responses.Add(statusCode500, new OpenApiResponse { Description = "Internal Server Error" });
        }
    }
}

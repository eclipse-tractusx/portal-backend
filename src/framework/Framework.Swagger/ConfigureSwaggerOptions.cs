using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        options.MapType(typeof(IFormFile), () => new OpenApiSchema { Type = "file", Format = "binary" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}, Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });

        options.OperationFilter<BaseStatusCodeFilter>();
        options.OperationFilter<AddAuthFilter>();
        options.OperationFilter<SwaggerDefaultValues>();
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = Assembly.GetEntryAssembly()?.FullName?.Split(',')[0],
            Version = description.ApiVersion.ToString(),
            License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}

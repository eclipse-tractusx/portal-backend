using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CatenaX.NetworkServices.Framework.Swagger;

public static class SwaggerGenConfiguration
{
    public static void SetupSwaggerGen(SwaggerGenOptions c, string version, string? tag)
    {
        c.SwaggerDoc(version, new OpenApiInfo {Title = tag, Version = version});
        c.OperationFilter<SwaggerFileOperationFilter>();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}, Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });

        var filePath = Path.Combine(System.AppContext.BaseDirectory, Assembly.GetCallingAssembly().FullName?.Split(',')[0] + ".xml");
        c.IncludeXmlComments(filePath);
        
        c.OperationFilter<SecurityRequirementsOperationFilter>();
        c.OperationFilter<AddAuthFilter>();
    }
}
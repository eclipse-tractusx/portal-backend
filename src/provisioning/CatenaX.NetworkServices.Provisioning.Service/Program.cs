using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Keycloak.Factory.Utils;
using CatenaX.NetworkServices.Provisioning.Library;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json.Serialization;

using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.ProvisioningEntities;

var VERSION = "v2";
var TAG = typeof(Program).Namespace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Kubernetes")
{
    var provider = new PhysicalFileProvider("/app/secrets");
    builder.Configuration.AddJsonFile(provider, "appsettings.json", optional: false, reloadOnChange: false);
}

builder.Services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

builder.Services.AddSwaggerGen(c => {
                    c.SwaggerDoc(VERSION, new OpenApiInfo { Title = TAG, Version = VERSION });
                    var filePath = Path.Combine(System.AppContext.BaseDirectory, Assembly.GetExecutingAssembly()?.FullName?.Split(',')[0] + ".xml");
                    c.IncludeXmlComments(filePath);
                });

builder.Services.AddAuthentication(x => {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options => {
                    builder.Configuration.Bind("JwtBearerOptions", options);
                    if (!options.RequireHttpsMetadata)
                    {
                        options.BackchannelHttpHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
                        };
                    }
                });

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
                .Configure<JwtBearerOptions>(options => builder.Configuration.Bind("JwtBearerOptions",options));

builder.Services.AddTransient<IIdentityProviderBusinessLogic,IdentityProviderBusinessLogic>()
                .AddTransient<IClientBusinessLogic,ClientBusinessLogic>()
                .AddTransient<IKeycloakFactory, KeycloakFactory>()
                .AddTransient<IProvisioningManager, ProvisioningManager>()
                .ConfigureKeycloakSettingsMap(builder.Configuration.GetSection("Keycloak"))
                .ConfigureProvisioningSettings(builder.Configuration.GetSection("Provisioning"));

builder.Services.AddTransient<IProvisioningDBAccess, ProvisioningDBAccess>();

builder.Services.AddDbContext<ProvisioningDBContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("ProvidioningDB")));


var app = builder.Build();

if (app.Configuration.GetValue<bool?>("DebugEnabled") != null && app.Configuration.GetValue<bool>("DebugEnabled"))
{
    app.UseDeveloperExceptionPage();
    KeycloakUntrustedCertExceptionHandler.ConfigureExceptions(app.Configuration.GetSection("Keycloak"));
    FlurlErrorLogging.ConfigureLogger(app.Services.GetRequiredService<ILogger<Program>>());
}
if (app.Configuration.GetValue<bool?>("SwaggerEnabled") != null && app.Configuration.GetValue<bool>("SwaggerEnabled"))
{
    app.UseSwagger( c => c.RouteTemplate = "/api/provisioning/swagger/{documentName}/swagger.{json|yaml}");
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint(string.Format("/api/provisioning/swagger/{0}/swagger.json",VERSION), string.Format("{0} {1}",TAG,VERSION));
        c.RoutePrefix = "api/provisioning/swagger";
    });
}

app.UseRouting();

app.UseMiddleware<GeneralHttpErrorHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

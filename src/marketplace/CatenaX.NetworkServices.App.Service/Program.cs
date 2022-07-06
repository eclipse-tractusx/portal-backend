using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory.Utils;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Mailing.Template;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using System.Text.Json.Serialization;
using CatenaX.NetworkServices.Framework.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

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

builder.Services.AddSwaggerGen(c => SwaggerGenConfiguration.SetupSwaggerGen(c, VERSION, TAG));

builder.Services.AddAuthentication(x =>
{
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
                    .Configure<JwtBearerOptions>(options => builder.Configuration.Bind("JwtBearerOptions", options));

builder.Services.AddTransient<IAppRepository, AppRepository>();
builder.Services.AddTransient<ICompanyAssignedAppsRepository, CompanyAssignedAppsRepository>();

builder.Services.AddDbContext<PortalDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("PortalDb")));
builder.Services.AddTransient<IAppsBusinessLogic, AppsBusinessLogic>();

builder.Services.AddTransient<IMailingService, MailingService>()
                .AddTransient<ISendMail, SendMail>()
                .AddTransient<ITemplateManager, TemplateManager>()
                .ConfigureTemplateSettings(builder.Configuration.GetSection(TemplateSettings.Position))
                .ConfigureMailSettings(builder.Configuration.GetSection(MailSettings.Position));

var app = builder.Build();

// Configure the HTTP request pipeline.

var debugEnabled = app.Configuration.GetValue<bool?>("DebugEnabled") != null && app.Configuration.GetValue<bool>("DebugEnabled");
if (debugEnabled)
{
    app.UseDeveloperExceptionPage();
    KeycloakUntrustedCertExceptionHandler.ConfigureExceptions(app.Configuration.GetSection("Keycloak"));
}
FlurlErrorHandler.ConfigureErrorHandler(app.Services.GetRequiredService<ILogger<Program>>(), debugEnabled);

if (app.Configuration.GetValue<bool?>("SwaggerEnabled") != null && app.Configuration.GetValue<bool>("SwaggerEnabled"))
{
    app.UseSwagger(c => c.RouteTemplate = "/api/apps/swagger/{documentName}/swagger.{json|yaml}");
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint(string.Format("/api/apps/swagger/{0}/swagger.json", VERSION), string.Format("{0} {1}", TAG, VERSION));
        c.RoutePrefix = "api/apps/swagger";
    });
}

app.UseRouting();

app.UseMiddleware<GeneralHttpErrorHandler>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

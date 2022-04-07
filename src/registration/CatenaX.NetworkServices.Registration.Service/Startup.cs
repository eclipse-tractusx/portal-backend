using CatenaX.NetworkServices.Framework.DBAccess;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Mailing.Template;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.Custodian;
using CatenaX.NetworkServices.Registration.Service.RegistrationAccess;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Keycloak.Factory.Utils;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.Provisioning.Library;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Newtonsoft.Json.Converters;
using CatenaX.NetworkServices.Registration.Service.Helper;

namespace CatenaX.NetworkServices.Registration.Service
{
    public class Startup
    {
        private static string TAG = typeof(Startup).Namespace;
        private static string VERSION = "v2";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson(options => 
                        options.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer( options => {
                Configuration.Bind("JwtBearerOptions",options);
                if (!options.RequireHttpsMetadata)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler {
                        ServerCertificateCustomValidationCallback = (a,b,c,d) => true
                    };
                }
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
                    .Configure<JwtBearerOptions>(options => Configuration.Bind("JwtBearerOptions",options));

            services.AddTransient<IAuthorizationHandler,ClaimRequestPathHandler>()
                    .AddAuthorization(option => {
                        option.AddPolicy("CheckTenant", policy =>
                        {
                            policy.AddRequirements(new ClaimRequestPathRequirement("tenant","tenant"));
                        });
                    })
                    .AddTransient<IHttpContextAccessor,HttpContextAccessor>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(VERSION, new OpenApiInfo { Title = TAG, Version = VERSION });
            });

            services.AddSwaggerGenNewtonsoftSupport();

            services.AddTransient<IRegistrationBusinessLogic, RegistrationBusinessLogic>()
                    .ConfigureRegistrationSettings(Configuration.GetSection("Registration"));

            services.AddTransient<IRegistrationDBAccess, RegistrationDBAccess>();

            services.AddCustodianService(Configuration.GetSection("Custodian"));

            services.AddTransient<IBPNAccess, BPNAccess>();
            services.AddHttpClient("bpn", c =>
            {
                c.BaseAddress = new Uri($"{ Configuration.GetValue<string>("BPN_Address")}");
            });

            services.AddTransient<IMailingService, MailingService>()
                    .AddTransient<ISendMail, SendMail>()
                    .AddTransient<ITemplateManager, TemplateManager>()
                    .ConfigureTemplateSettings(Configuration.GetSection(TemplateSettings.Position))
                    .ConfigureMailSettings(Configuration.GetSection(MailSettings.Position));

            services.AddTransient<IKeycloakFactory, KeycloakFactory>()
                    .ConfigureKeycloakSettingsMap(Configuration.GetSection("Keycloak"));

            services.AddTransient<IProvisioningManager, ProvisioningManager>()
                    .ConfigureProvisioningSettings(Configuration.GetSection("Provisioning"));

            services.AddTransient<IDBConnectionFactories, PostgreConnectionFactories>()
                    .ConfigureDBConnectionSettingsMap(Configuration.GetSection("DatabaseAccess"));

            services.AddTransient<IPortalBackendDBAccess, PortalBackendDBAccess>();

            services.AddDbContext<PortalDBContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PortalDB")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (Configuration.GetValue<bool?>("DebugEnabled") != null && Configuration.GetValue<bool>("DebugEnabled"))
            {
                app.UseDeveloperExceptionPage();
                KeycloakUntrustedCertExceptionHandler.ConfigureExceptions(Configuration.GetSection("Keycloak"));
                FlurlErrorLogging.ConfigureLogger(logger);
            }
            if (Configuration.GetValue<bool?>("SwaggerEnabled") != null && Configuration.GetValue<bool>("SwaggerEnabled"))
            {
                app.UseSwagger( c => c.RouteTemplate = "/api/registration/swagger/{documentName}/swagger.{json|yaml}");
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(string.Format("/api/registration/swagger/{0}/swagger.json",VERSION), string.Format("{0} {1}",TAG,VERSION));
                    c.RoutePrefix = "api/registration/swagger";
                });
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<GeneralErrorHandler>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

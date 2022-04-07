using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CatenaX.NetworkServices.Framework.DBAccess;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Keycloak.Factory.Utils;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Service.BusinessLogic;

namespace CatenaX.NetworkServices.Provisioning.Service
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
            services.AddControllers();
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

            services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
                    .Configure<JwtBearerOptions>(options => Configuration.Bind("JwtBearerOptions",options));

            services.AddSwaggerGen(c => {
                c.SwaggerDoc(VERSION, new OpenApiInfo { Title = TAG, Version = VERSION });
            });
   
            services.AddTransient<IIdentityProviderBusinessLogic,IdentityProviderBusinessLogic>()
                    .AddTransient<IClientBusinessLogic,ClientBusinessLogic>()
                    .AddTransient<IKeycloakFactory, KeycloakFactory>()
                    .AddTransient<IProvisioningManager, ProvisioningManager>()
                    .ConfigureKeycloakSettingsMap(Configuration.GetSection("Keycloak"))
                    .ConfigureProvisioningSettings(Configuration.GetSection("Provisioning"));

            services.AddTransient<IProvisioningDBAccess, ProvisioningDBAccess>();

            services.AddTransient<IDBConnectionFactories, PostgreConnectionFactories>()
                    .ConfigureDBConnectionSettingsMap(Configuration.GetSection("DatabaseAccess"));

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
                app.UseSwagger( c => c.RouteTemplate = "/api/provisioning/swagger/{documentName}/swagger.{json|yaml}");
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint(string.Format("/api/provisioning/swagger/{0}/swagger.json",VERSION), string.Format("{0} {1}",TAG,VERSION));
                    c.RoutePrefix = "api/provisioning/swagger";
                });
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

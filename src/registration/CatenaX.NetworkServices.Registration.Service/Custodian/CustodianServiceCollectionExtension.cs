
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;

namespace CatenaX.NetworkServices.Registration.Service.Custodian
{
    public static class CustodianServiceCollectionExtension
    {
        public static IServiceCollection AddCustodianService(this IServiceCollection services, IConfigurationSection section)
        {
            services.Configure<CustodianSettings>(x => section.Bind(x));
            var sp = services.BuildServiceProvider();
            var settings = sp.GetRequiredService<IOptions<CustodianSettings>>();
            services.AddHttpClient("custodian", c =>
            {
                c.BaseAddress = new Uri(settings.Value.BaseAdress);
            }); 
            services.AddHttpClient("custodianAuth", c =>
            {
                c.BaseAddress = new Uri(settings.Value.KeyCloakTokenAdress);
            });
            services.AddTransient<ICustodianService, CustodianService>();

            return services;
        }
    }
}

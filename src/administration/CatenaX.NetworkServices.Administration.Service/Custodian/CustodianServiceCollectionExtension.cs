
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.Custodian;

public static class CustodianServiceCollectionExtension
{
    public static IServiceCollection AddCustodianService(this IServiceCollection services, IConfigurationSection section)
    {
        services.Configure<CustodianSettings>(x =>
            {
                section.Bind(x);
                if(String.IsNullOrWhiteSpace(x.Username))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Username)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.Password))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Password)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.ClientId))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.ClientId)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.GrantType))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.GrantType)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.ClientSecret))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.ClientSecret)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.Scope))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.Scope)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.KeyCloakTokenAdress))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.KeyCloakTokenAdress)} must not be null or empty");
                }
                if(String.IsNullOrWhiteSpace(x.BaseAdress))
                {
                    throw new Exception($"{nameof(CustodianSettings)}: {nameof(x.BaseAdress)} must not be null or empty");
                }
            });
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Framework.Web;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

namespace Offers.Library.DependencyInjection;

public static class OfferSetupServiceCollectionExtension
{
    public static IServiceCollection AddOfferSetupService(this IServiceCollection services)
    {
        services.AddTransient<LoggingHandler<OfferSetupService>>();
        services.AddHttpClient(nameof(OfferSetupService), c =>
        {
        }).AddHttpMessageHandler<LoggingHandler<OfferSetupService>>();
        services.AddTransient<IOfferSetupService, OfferSetupService>();

        return services;
    }
}
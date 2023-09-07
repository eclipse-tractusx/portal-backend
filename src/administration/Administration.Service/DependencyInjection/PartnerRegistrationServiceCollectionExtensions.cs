using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;

public static class PartnerRegistrationServiceCollectionExtensions
{
    public static IServiceCollection AddPartnerRegistration(this IServiceCollection services, IConfiguration configuration) =>
        services
            .ConfigurePartnerRegistrationSettings(configuration.GetSection("Network2Network"))
            .AddOnboardingServiceProviderService(configuration)
            .AddTransient<INetworkBusinessLogic, NetworkBusinessLogic>();
}

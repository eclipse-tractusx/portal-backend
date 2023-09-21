using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;

public static class PartnerRegistrationServiceCollectionExtensions
{
    public static IServiceCollection AddPartnerRegistration(this IServiceCollection services, IConfigurationSection section) =>
        services
            .ConfigurePartnerRegistrationSettings(section)
            .AddTransient<INetworkBusinessLogic, NetworkBusinessLogic>();
}

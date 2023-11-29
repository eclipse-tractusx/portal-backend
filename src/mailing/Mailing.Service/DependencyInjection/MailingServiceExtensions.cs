using Microsoft.Extensions.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Service.DependencyInjection;

public static class MailingServiceExtensions
{
    public static IServiceCollection AddMailingService(this IServiceCollection services)
    {
        return services
            .AddTransient<IRoleBaseMailService, RoleBaseMailService>();
    }
}

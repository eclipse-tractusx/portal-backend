using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public class IdentityClaimHandlerSettings
{
    public string? ClientIdClaim { get; set; }
}

public static class IdentityClaimHandlerStartupExtensions
{
    public static IServiceCollection AddClientIdClaimConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IdentityClaimHandlerSettings>()
            .Bind(configuration.GetSection("ClaimHandler"))
            .ValidateOnStart();

        return services;
    }
}

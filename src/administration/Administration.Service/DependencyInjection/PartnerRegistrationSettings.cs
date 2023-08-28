using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;

public class PartnerRegistrationSettings
{
    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> InitialRoles { get; set; } = null!;
}

public static class PartnerRegistrationSettingsExtensions
{
    public static IServiceCollection ConfigurePartnerRegistrationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<PartnerRegistrationSettings>()
            .Bind(section)
            .ValidateDistinctValues(section)
            .ValidateEnumEnumeration(section)
            .ValidateOnStart();
        return services;
    }
}

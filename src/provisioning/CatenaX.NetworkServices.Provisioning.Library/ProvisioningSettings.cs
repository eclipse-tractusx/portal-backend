using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningSettings
    {
        public string CentralRealm { get; set; }
        public string CentralRealmId { get; set; }
        public string IdpPrefix { get; set; }
        public string ClientPrefix { get; set; }
        public string MappedIdpAttribute { get; set; }
        public string MappedCompanyAttribute { get; set; }
        public string UserNameMapperTemplate { get; set; }
        public IDictionary<string,IEnumerable<string>> InvitedUserInitialRoles { get; set; }
    }

    public static class ProvisioningSettingsExtension
    {
        public static IServiceCollection ConfigureProvisioningSettings(
            this IServiceCollection services,
            IConfigurationSection section
            )
        {
            return services.Configure<ProvisioningSettings>(x => section.Bind(x));
        }
    }
}

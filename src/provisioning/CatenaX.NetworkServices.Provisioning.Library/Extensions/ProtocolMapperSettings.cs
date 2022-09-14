using CatenaX.NetworkServices.Framework.ErrorHandling;
using Keycloak.Net.Models.ProtocolMappers;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningSettings
{
    public ProtocolMapper ClientProtocolMapper { get; set; } = null!;
    
    public ProvisioningSettings ValidateProtocolMapperTemplate()
    {
        new ConfigurationValidation<ProvisioningSettings>()
            .NotNull(ClientProtocolMapper, () => nameof(ClientProtocolMapper));
        return this;
    }
}

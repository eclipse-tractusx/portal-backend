using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;

namespace Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;

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

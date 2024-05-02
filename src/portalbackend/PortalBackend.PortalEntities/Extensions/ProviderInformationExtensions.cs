using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class ProviderInformationExtensions
{
    public static string GetProviderName(this ProviderInformationId providerInformationId)
    {
        return providerInformationId switch
        {
            ProviderInformationId.KEYCLOAK => "Keycloak",
            ProviderInformationId.SAP_DIM => "Sap Dim",
            _ => throw new ArgumentOutOfRangeException($"{providerInformationId} is not supported")
        };
    }
}

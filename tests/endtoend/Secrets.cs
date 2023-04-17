using Microsoft.Extensions.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class Secrets
{
    public string TempMailApiKey { get; set; }
    public string InterfaceHealthCheckTechClientId { get; set; }
    public string InterfaceHealthCheckTechClientSecret { get; set; }
    public string ClearingHouseClientId { get; set; }
    public string ClearingHouseClientSecret { get; set; }
    public string PortalUserName { get; set; }
    public string PortalUserPassword { get; set; }

    public Secrets()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .AddEnvironmentVariables()
            .Build();

        TempMailApiKey = configuration["TEMPMAIL_APIKEY"];
        InterfaceHealthCheckTechClientId = configuration["INTERFACE_HEALTH_CHECK_TECH_CLIENT_ID"];
        InterfaceHealthCheckTechClientSecret = configuration["INTERFACE_HEALTH_CHECK_TECH_CLIENT_SECRET"];
        ClearingHouseClientId = configuration["CLEARING_HOUSE_CLIENT_ID"];
        ClearingHouseClientSecret = configuration["CLEARING_HOUSE_CLIENT_SECRET"];
        PortalUserName = configuration["PORTAL_USER_NAME"];
        PortalUserPassword = configuration["PORTAL_USER_PASSWORD"];
    }
}

using Microsoft.Extensions.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

public class Secrets
{
    public string TempMailApiKey { get; set; }
    public string OperatorUserName { get; set; }
    public string OperatorUserPassword { get; set; }
    public string TechUserName { get; set; }
    public string TechUserPassword { get; set; }
    public string InterfaceHealthCheckTechUserName { get; set; }
    public string InterfaceHealthCheckTechUserPassword { get; set; }
    public string InterfaceHealthCheckTechUserNameInt { get; set; }
    public string InterfaceHealthCheckTechUserPasswordInt { get; set; }
    public string PortalUserName { get; set; }
    public string PortalUserPassword { get; set; }

    public Secrets()
    {
        var configuration = new ConfigurationBuilder()
        .AddUserSecrets<Secrets>()
        .Build();
        var authFlow = configuration.GetSection("AuthFlow");
        TempMailApiKey = authFlow["TempMailApiKey"];
        OperatorUserName = authFlow["OperatorUserName"];
        OperatorUserPassword = authFlow["OperatorUserPassword"];
        TechUserName = authFlow["TechUserName"];
        TechUserPassword = authFlow["TechUserPassword"];
        InterfaceHealthCheckTechUserName = authFlow["InterfaceHealthCheckTechUserName"];
        InterfaceHealthCheckTechUserPassword = authFlow["InterfaceHealthCheckTechUserPassword"];
        InterfaceHealthCheckTechUserNameInt = authFlow["InterfaceHealthCheckTechUserNameInt"];
        InterfaceHealthCheckTechUserPasswordInt = authFlow["InterfaceHealthCheckTechUserPasswordInt"];
        PortalUserName = authFlow["PortalUserName"];
        PortalUserPassword = authFlow["PortalUserPassword"];
    }
}

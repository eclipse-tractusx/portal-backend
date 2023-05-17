using Microsoft.Extensions.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

public class Secrets
{
    public string TempMailApiKey { get; set; }
    public string OperatorUserName { get; set; }
    public string OperatorUserPassword { get; set; }

    public Secrets()
    {
        var configuration = new ConfigurationBuilder()
        .AddUserSecrets<Secrets>()
        .Build();
        var authFlow = configuration.GetSection("AuthFlow");
        TempMailApiKey = authFlow["TempMailApiKey"];
        OperatorUserName = authFlow["OperatorUserName"];
        OperatorUserPassword = authFlow["OperatorUserPassword"];
    }
}

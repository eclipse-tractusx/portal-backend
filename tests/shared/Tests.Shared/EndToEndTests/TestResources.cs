using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Tests.Shared.EndToEndTests;

public static class TestResources
{
    public static readonly string Env;
    public static readonly string BaseUrl;
    public static readonly string ClearingHouseUrl;
    public static readonly string NotificationOfferId;
    public static readonly string SdFactoryBaseUrl;
    public static readonly string WalletBaseUrl;

    static TestResources()
    {
        var projectDir = Directory.GetParent(GetSourceFilePathName()).FullName;
        var configPath = Path.Combine(projectDir, "appsettings.EndToEndTests.json");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath)
            .Build();
        Env = configuration.GetSection("Environment").Value;
        NotificationOfferId = configuration.GetSection("NotificationOfferId").Value;
        BaseUrl = $"https://portal-backend.{Env}.demo.catena-x.net";
        ClearingHouseUrl = $"https://validation.{Env}.dih-cloud.com";
        SdFactoryBaseUrl = $"https://sdfactory.{Env}.demo.catena-x.net";
        WalletBaseUrl = $"https://managed-identity-wallets.{Env}.demo.catena-x.net";
    }

    public static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null) //
        => callerFilePath ?? "";
}
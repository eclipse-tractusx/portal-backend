namespace Tests.Shared.EndToEndTests;

public static class TestResources
{
    private static readonly string Environment = "dev";
    public static readonly string BaseUrl;
    public static readonly string ClearingHouseUrl;
    public static readonly string NotificationOfferId = "9b957704-3505-4445-822c-d7ef80f27fcd";
    public static readonly string SdFactoryBaseUrl;
    public static readonly string WalletBaseUrl;

    static TestResources()
    {
        BaseUrl = $"https://portal-backend.{Environment}.demo.catena-x.net";
        ClearingHouseUrl = $"https://validation.{Environment}.dih-cloud.com";
        SdFactoryBaseUrl = $"https://sdfactory.{Environment}.demo.catena-x.net";
        WalletBaseUrl = $"https://managed-identity-wallets.{Environment}.demo.catena-x.net";
    }
}
namespace Framework.Models;

public static class PortalClaimTypes
{
    private const string Base = "https://catena-x.net//schema/2023/05/identity/claims";
    public const string Sub = "sub";
    public const string CompanyId = $"{Base}/company_id";
    public const string IdentityId = $"{Base}/identity_id";
    public const string IdentityType = $"{Base}/identity_type";
}

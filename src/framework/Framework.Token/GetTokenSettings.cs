namespace Org.CatenaX.Ng.Portal.Backend.Framework.Token;

public record GetTokenSettings(string HttpClientName, string Username, string Password, string ClientId, string GrantType, string ClientSecret, string Scope);

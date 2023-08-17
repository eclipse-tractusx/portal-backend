using EndToEnd.Tests;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class AuthFlow
{
    private const string RequestHeaderAccept =
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
    private const string RequestHeaderAcceptEncoding = "gzip, deflate, br";
    private const string RequestHeaderAcceptLanguage = "en-GB,en;q=0.9,de;q=0.8,en-US;q=0.7";

    private readonly string _basePortalUrl = TestResources.BasePortalUrl;
    private readonly string _baseCentralIdpUrl = TestResources.BaseCentralIdpUrl;

    private readonly string _startCentralIdpEndpoint = "auth/realms/CX-Central/protocol/openid-connect/auth";
    private readonly string _companyCentralIdpEndpoint = "auth/realms/CX-Central/broker";
    private readonly string _tokenCentralIdpEndpoint = "auth/realms/CX-Central/protocol/openid-connect/token";
    private static List<string>? _cookies;

    private readonly HttpClientHandler _httpClientHandler = new() { AllowAutoRedirect = false };

    private readonly HttpClient _client;
    private readonly string _companyName;

    public AuthFlow(string companyName)
    {
        _client = new HttpClient(_httpClientHandler);
        _companyName = companyName;
    }

    private List<CentralidpCompany> StartCentralIdp()
    {
        var queryParams = new Dictionary<string, object>
        {
            { "client_id", "Cl2-CX-Portal" },
            { "redirect_uri", _basePortalUrl },
            { "response_mode", "fragment" },
            { "response_type", "code" },
            { "scope", "openid" },
        };
        var response = Given()
            .DisableSslCertificateValidation()
            .QueryParams(queryParams)
            .When()
            .Get($"{_baseCentralIdpUrl}/{_startCentralIdpEndpoint}")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var htmlString = response.Content.ReadAsStringAsync().Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlString);
        _cookies = response.Headers.GetValues("Set-Cookie").ToList();
        var companies = DataHandleHelper.DeserializeData<List<CentralidpCompany>>(doc.DocumentNode.Descendants("pre")
            .Single()
            .InnerText);
        if (companies is null)
        {
            throw new Exception("Companies were not found.");
        }

        return companies;
    }

    private async Task<string> GetCompanySharedIdpUrl(List<CentralidpCompany> companies)
    {
        var company = companies.FirstOrDefault(c => c.Name.Equals(_companyName) | c.Alias.Equals(_companyName));
        if (company is null)
        {
            throw new Exception($"Authentication failed: Company {_companyName} was not found.");
        }

        var companyUrl = HttpUtility.HtmlDecode(company.Url);
        var regexTabId = new Regex(@"tab_id=([\S]*)&session_code", RegexOptions.IgnoreCase);
        var regexSessionCode = new Regex(@"session_code=([\S]*)", RegexOptions.IgnoreCase);
        var regexCompany = new Regex(@"broker/([\S]*)/login", RegexOptions.IgnoreCase);
        var tabId = regexTabId.Matches(companyUrl).First().Groups[1].Value;
        var sessionCode = regexSessionCode.Matches(companyUrl).First().Groups[1].Value;
        var companyNameInUrl = regexCompany.Matches(companyUrl).First().Groups[1].Value;

        var uri = new UriBuilder($"{_baseCentralIdpUrl}/{_companyCentralIdpEndpoint}/{companyNameInUrl}/login");
        uri.Query = $"client_id=Cl2-CX-Portal&tab_id={tabId}&session_code={sessionCode}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Host", _baseCentralIdpUrl.Replace("https://", ""));
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Cookie",
            $"{_cookies?[0].Split(";")[0]}; {_cookies?[1].Split(";")[0]}; {_cookies?[2].Split(";")[0]}");

        var response = await _client.SendAsync(request);
        var companySharedIdpUrl = response.Headers.Location?.AbsoluteUri;
        if (companySharedIdpUrl is null)
        {
            throw new Exception($"Authentication failed: The sharedidp url for the company {_companyName} was not found");
        }

        return companySharedIdpUrl;
    }

    private async Task<string> GetAuthUrlFromSharedIdp(string sharedIdpUrl)
    {
        var uri = new UriBuilder($"{sharedIdpUrl}");
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept", RequestHeaderAccept);
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);

        var response = await _client.SendAsync(request);
        var htmlString = response.Content.ReadAsStringAsync().Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlString);
        var authUrl = doc.DocumentNode.Descendants("form").Single().Attributes.AttributesWithName("action").Single()
            .Value;
        if (authUrl is null)
        {
            throw new Exception($"Authentication failed: The authUrl was not found");
        }

        return authUrl;
    }

    private async Task<string> AuthenticateInSharedIdp(string sharedIdpUrl, string username, string password)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(sharedIdpUrl)}");
        var request = new HttpRequestMessage(HttpMethod.Post, uri.Uri.AbsoluteUri);
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("companyName", _companyName),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("credentialId", ""),
        });
        request.Headers.Add("Accept", RequestHeaderAccept);
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Origin", "null");
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        var authCodeOrUpdatePasswordUrl = response.Headers.Location?.AbsoluteUri;
        if (authCodeOrUpdatePasswordUrl is null)
        {
            throw new Exception("Authentication failed: the url for getting the auth code or updating the password was not found.");
        }

        return authCodeOrUpdatePasswordUrl;
    }

    private async Task<string> GetAuthCodeFromCentralIdp(string authCodeUrl)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(authCodeUrl)}");
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept", RequestHeaderAccept);
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Cookie",
            $"{_cookies?[0].Split(";")[0]}; {_cookies?[1].Split(";")[0]}; {_cookies?[2].Split(";")[0]}");

        var response = await _client.SendAsync(request);
        var tokenUri = new UriBuilder($"{HttpUtility.HtmlDecode(response.Headers.Location?.AbsoluteUri)}");
        var r = new Regex(@"code=([\S]*)", RegexOptions.IgnoreCase);
        return r.Matches(tokenUri.Uri.AbsoluteUri).First().Groups[1].Value;
    }

    private async Task<string> GetUpdatePasswordUrl(string forwardUpdatePasswordUrl)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(forwardUpdatePasswordUrl)}");
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept", RequestHeaderAccept);
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Cookie",
            $"{_cookies?[0].Split(";")[0]}; {_cookies?[1].Split(";")[0]}; {_cookies?[2].Split(";")[0]}");

        var response = await _client.SendAsync(request);
        var htmlString = response.Content.ReadAsStringAsync().Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlString);
        var updatePasswordUrl = doc.DocumentNode.Descendants("form").Single().Attributes.AttributesWithName("action")
            .Single()
            .Value;
        if (updatePasswordUrl is null)
        {
            throw new Exception("Authentication failed: The update password url was not found");
        }

        return updatePasswordUrl;
    }

    private async Task<string> UpdatePassword(string updatePasswordUrl, string username, string oldPassword,
        string newPassword)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(updatePasswordUrl)}");
        var request = new HttpRequestMessage(HttpMethod.Post, uri.Uri.AbsoluteUri);
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", oldPassword),
            new KeyValuePair<string, string>("password-new", newPassword),
            new KeyValuePair<string, string>("password-confirm", newPassword),
        });
        request.Headers.Add("Accept", RequestHeaderAccept);
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Origin", "null");
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        var centralIdpUrl = response.Headers.Location?.AbsoluteUri;
        if (centralIdpUrl is null)
        {
            throw new Exception("Authentication failed: The centralidp url for getting the auth code was not found.");
        }

        return centralIdpUrl;
    }

    private async Task<string> GetTokenWithAuthCode(string authCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_baseCentralIdpUrl}/{_tokenCentralIdpEndpoint}");
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", "Cl2-CX-Portal"),
            new KeyValuePair<string, string>("redirect_uri", TestResources.BasePortalUrl),
        });
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", RequestHeaderAcceptLanguage);
        request.Headers.Add("Accept-Encoding", RequestHeaderAcceptEncoding);
        request.Headers.Add("Origin", TestResources.BasePortalUrl);
        request.Headers.Add("Referer", TestResources.BasePortalUrl);
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        var content = response.Content.ReadAsStringAsync().Result;
        var token = DataHandleHelper.DeserializeData<Token>(content)?.AccessToken;
        if (token is null)
        {
            throw new Exception("Token was not found.");
        }

        return token;
    }

    public async Task<string> GetAccessToken(string username, string password)
    {
        var companies = StartCentralIdp();
        var sharedIdpCompanyUrl = await GetCompanySharedIdpUrl(companies);
        var authUrlFromSharedIdp = await GetAuthUrlFromSharedIdp(sharedIdpCompanyUrl);
        var authenticateInCentralIdpUrl =
            await AuthenticateInSharedIdp(authUrlFromSharedIdp, username, password);
        var authCode = await GetAuthCodeFromCentralIdp(authenticateInCentralIdpUrl);
        return await GetTokenWithAuthCode(authCode);
    }

    public async Task<string> UpdatePasswordAndGetAccessToken(string username, string password, string newPassword)
    {
        var companies = StartCentralIdp();
        var sharedIdpCompanyUrl = await GetCompanySharedIdpUrl(companies);
        var authUrlFromSharedIdp = await GetAuthUrlFromSharedIdp(sharedIdpCompanyUrl);
        var forwardUpdatePasswordUrl = await AuthenticateInSharedIdp(authUrlFromSharedIdp, username, password);
        var updatePasswordUrl = await GetUpdatePasswordUrl(forwardUpdatePasswordUrl);
        var authenticateInCentralIdpUrl =
            await UpdatePassword(updatePasswordUrl, username, password, newPassword);
        var authCode = await GetAuthCodeFromCentralIdp(authenticateInCentralIdpUrl);
        return await GetTokenWithAuthCode(authCode);
    }
}

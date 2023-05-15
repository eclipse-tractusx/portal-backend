using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured;

public class AuthFlow
{
    private string _basePortalUrl = "https://portal.dev.demo.catena-x.net/";
    private string _baseCentralIdpUrl = "https://centralidp.dev.demo.catena-x.net/";

    private string _startCentralidpEndpoint = "auth/realms/CX-Central/protocol/openid-connect/auth";

    private string _companyCentralIdpEndpoint = "auth/realms/CX-Central/broker";
    private static List<string> cookies;

    private HttpClientHandler _httpClientHandler = new ()
        { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };

    private HttpClient _client;
    private string operatorUserName;
    private string operatorUserPassword;
    private static string _companyName;

    public AuthFlow()
    {
        _client = new HttpClient(_httpClientHandler);
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        operatorUserName = configuration.GetValue<string>("Secrets:OperatorUserName");
        operatorUserPassword = configuration.GetValue<string>("Secrets:OperatorUserPassword");
    }

    private List<CentralidpCompany>? StartCentralIdp()
    {
        Dictionary<string, object> queryParams = new Dictionary<string, object>
        {
            { "client_id", "Cl2-CX-Portal" },
            { "redirect_uri", _basePortalUrl },
            { "response_mode", "fragment" },
            { "response_type", "code" },
            { "scope", "openid" },
        };
        var response = Given()
            .RelaxedHttpsValidation()
            .QueryParams(queryParams)
            .When()
            .Get($"{_baseCentralIdpUrl}{_startCentralidpEndpoint}")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        var htmlString = response.Content.ReadAsStringAsync().Result;
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlString);
        cookies = response.Headers.GetValues("Set-Cookie").ToList();
        return JsonConvert.DeserializeObject<List<CentralidpCompany>>(doc.DocumentNode.Descendants("pre").Single()
            .InnerText);
    }

    private async Task<string?> GetCompanySharedIdpUrl()
    {
        var companies = StartCentralIdp();
        if (companies is null)
        {
            throw new Exception("Companies were not found.");
        }

        var company = companies.FirstOrDefault(c => c.Name.Equals(_companyName) | c.Alias.Equals(_companyName));
        if (company is null)
        {
            throw new Exception($"Company {_companyName} was not found.");
        }

        var companyUrl = HttpUtility.HtmlDecode(company.Url);
        Regex regexTabId = new Regex(@"tab_id=([\S]*)&session_code", RegexOptions.IgnoreCase);
        Regex regexSessionCode = new Regex(@"session_code=([\S]*)", RegexOptions.IgnoreCase);
        string tabId = regexTabId.Matches(companyUrl).First().Groups[1].Value;
        string sessionCode = regexSessionCode.Matches(companyUrl).First().Groups[1].Value;

        var uri = new UriBuilder($"{_baseCentralIdpUrl}{_companyCentralIdpEndpoint}/{_companyName}/login");
        uri.Query = $"client_id=Cl2-CX-Portal&tab_id={tabId}&session_code={sessionCode}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Host", "centralidp.dev.demo.catena-x.net");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("Cookie",
            $"{cookies[0].Split(";")[0]}; {cookies[1].Split(";")[0]}; {cookies[2].Split(";")[0]}");

        var response = await _client.SendAsync(request);
        var companySharedIdpUrl = response.Headers.Location?.AbsoluteUri;
        if (companySharedIdpUrl is null)
        {
            throw new Exception($"The sharedidp url for the company {_companyName} was not found");
        }
        return companySharedIdpUrl;
    }

    private async Task<string?> CompanySharedIdp(string sharedIdpUrl)
    {
        var uri = new UriBuilder($"{sharedIdpUrl}");
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,de;q=0.8,en-US;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");

        var response = await _client.SendAsync(request);
        var htmlString = response.Content.ReadAsStringAsync().Result;
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlString);
        return doc.DocumentNode.Descendants("form").Single().Attributes.AttributesWithName("action").Single().Value;
    }

    private async Task<string> GetAuthenticateUrl(string sharedIdpUrl)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(sharedIdpUrl)}");
        var request = new HttpRequestMessage(HttpMethod.Post, uri.Uri.AbsoluteUri);
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("companyName", _companyName),
            new KeyValuePair<string, string>("username", operatorUserName),
            new KeyValuePair<string, string>("password", operatorUserPassword),
            new KeyValuePair<string, string>("credentialId", ""),
        });
        request.Headers.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,de;q=0.8,en-US;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("Origin", "null");
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        return response.Headers.Location?.AbsoluteUri;
    }

    private async Task<string> GetAuthCode(string authenticateUrl)
    {
        // var companyUrl = HttpUtility.HtmlDecode(company.Url);
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(authenticateUrl)}");
        // uri.Query = $"{uri}";
        var request = new HttpRequestMessage(HttpMethod.Get, uri.Uri.AbsoluteUri);
        request.Headers.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,de;q=0.8,en-US;q=0.7");
        // request.Headers.Add("Host", "centralidp.dev.demo.catena-x.net");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("Cookie",
            $"{cookies[0].Split(";")[0]}; {cookies[1].Split(";")[0]}; {cookies[2].Split(";")[0]}");

        var response = await _client.SendAsync(request);
        return response.Headers.Location?.AbsoluteUri;
    }

    private async Task<string> GetTokenWithAuthCode(string tokenUrl)
    {
        var uri = new UriBuilder($"{HttpUtility.HtmlDecode(tokenUrl)}");
        Regex r = new Regex(@"code=([\S]*)", RegexOptions.IgnoreCase);
        string authCode = r.Matches(uri.Uri.AbsoluteUri).First().Groups[1].Value;
        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://centralidp.dev.demo.catena-x.net/auth/realms/CX-Central/protocol/openid-connect/token");
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", "Cl2-CX-Portal"),
            new KeyValuePair<string, string>("redirect_uri", "https://portal.dev.demo.catena-x.net/"),
        });
        request.Headers.Add("Accept",
            "*/*");
        request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,de;q=0.8,en-US;q=0.7");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("Origin", "https://portal.dev.demo.catena-x.net");
        request.Headers.Add("Referer", "https://portal.dev.demo.catena-x.net/");
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        var content = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<Token>(content).AccessToken;
    }

    public async Task<string> GetAccessToken(string companyName)
    {
        _companyName = companyName;
        var sharedIdpCompanyUrl = await GetCompanySharedIdpUrl();
        var authCodeUrl = await CompanySharedIdp(sharedIdpCompanyUrl);
        var authenticateUrl = await GetAuthenticateUrl(authCodeUrl);
        var tokenUrl = await GetAuthCode(authenticateUrl);
        return await GetTokenWithAuthCode(tokenUrl);
    }
}
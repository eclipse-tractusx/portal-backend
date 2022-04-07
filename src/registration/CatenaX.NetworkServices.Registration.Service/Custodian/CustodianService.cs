using CatenaX.NetworkServices.Registration.Service.Custodian.Models;
using CatenaX.NetworkServices.Registration.Service.CustomException;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Org.BouncyCastle.Bcpg;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.Custodian
{
    public class CustodianService : ICustodianService
    {
        private readonly CustodianSettings _settings;
        private readonly HttpClient _custodianHttpClient;
        private readonly HttpClient _custodianAuthHttpClient;
        private readonly ILogger<CustodianService> _logger;

        public CustodianService(ILogger<CustodianService> logger,IHttpClientFactory httpFactory, IOptions<CustodianSettings> settings)
        {
            _settings = settings.Value;
            _custodianHttpClient = httpFactory.CreateClient("custodian");
            _custodianAuthHttpClient = httpFactory.CreateClient("custodianAuth");
            _logger = logger;
        }


        public async Task CreateWallet(string bpn, string name)
        {
            var token = await GetToken();
            _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestBody = new { name = name, bpn = bpn };
            var stringContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var result = await _custodianHttpClient.PostAsync($"/api/wallets", stringContent);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"Error on creating Wallet HTTP Response Code {result.StatusCode}");
                throw new ServiceException($"Access to Custodian Failed with Status Code {result.StatusCode}", result.StatusCode);
            }
        }

        public async Task<List<GetWallets>> GetWallets()
        {
            var response = new List<GetWallets>();
            var token = await GetToken();
            _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var result = await _custodianHttpClient.GetAsync("/api/wallets");
            if (result.IsSuccessStatusCode)
            {
                response.AddRange(JsonSerializer.Deserialize<List<GetWallets>>(await result.Content.ReadAsStringAsync()));
            }
            else
            {
                _logger.LogInformation($"Error on retrieveing Wallets HTTP Response Code {result.StatusCode}");
            }
            return response;
        }

        public async Task<string> GetToken()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("username", _settings.Username);
            parameters.Add("password", _settings.Password);
            parameters.Add("client_id", _settings.ClientId);
            parameters.Add("grant_type", _settings.GrantType);
            parameters.Add("client_secret", _settings.ClientSecret);
            parameters.Add("scope", _settings.Scope);
            var content = new FormUrlEncodedContent(parameters);
            var response = await _custodianAuthHttpClient.PostAsync("", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Token could not be retrieved");
            }
            var responseObject = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync());
            return responseObject.access_token;
        }
    }
}


using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.CustomException;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.BPN
{
    public class BPNAccess : IBPNAccess
    {
        private readonly HttpClient _httpClient;

        public BPNAccess(IHttpClientFactory _httpFactory)
        {
            _httpClient = _httpFactory.CreateClient("bpn");
        }

        public async Task<List<FetchBusinessPartnerDto>> FetchBusinessPartner(string bpn, string token)
        {
            var response = new List<FetchBusinessPartnerDto>();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var result = await _httpClient.GetAsync($"api/catena/business-partner/{bpn}");
            if (result.IsSuccessStatusCode)
            {
                    var body = JsonSerializer.Deserialize<FetchBusinessPartnerDto>(await result.Content.ReadAsStringAsync());
                    response.Add(body);
            }
            else
            {
                throw new ServiceException($"Access to BPN Failed with Status Code {result.StatusCode}", result.StatusCode);
            }
            
            return response;
        }
    }
}

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Administration.Service.Tests.EnpointSetup
{
    public class RegistrationEndpoint
    {
        private readonly HttpClient client;

        public static string Path => Paths.Registration;

        public RegistrationEndpoint(HttpClient client)
        {
            this.client = client;
        }

        public async Task<HttpResponseMessage> GetCompanyWithAddressAsync(Guid applicationId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/application/{applicationId}/companyDetailsWithAddress");
            return await this.client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> GetApplicationDetailsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}/applications");
            return await this.client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> ApprovePartnerRequest(Guid applicationId)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{Path}/application/{applicationId}/approveRequest");
            return await this.client.SendAsync(request);
        }
    }
}

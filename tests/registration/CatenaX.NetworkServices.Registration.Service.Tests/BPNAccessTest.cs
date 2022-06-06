using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using FakeItEasy;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CatenaX.NetworkServices.Registration.Service.Tests
{
    public class BPNAccessTest
    {
        private readonly IFixture _fixture;

        public BPNAccessTest()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        }

        private void ConfigureHttpClientFactoryFixture(HttpResponseMessage httpResponseMessage)
        {
            var messageHandler = A.Fake<HttpMessageHandler>();
            A.CallTo(messageHandler) // mock protected method
                .Where(x => x.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Returns(httpResponseMessage);
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
            _fixture.Inject(httpClient);

            var httpClientFactory = _fixture.Freeze<Fake<IHttpClientFactory>>();
            A.CallTo(() => httpClientFactory.FakedObject.CreateClient("bpn")).Returns(httpClient);
        }

        [Fact]
        public async Task FetchBusinessPartner_Success()
        {
            var resultSet = _fixture.Create<FetchBusinessPartnerDto>();
            ConfigureHttpClientFactoryFixture(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(resultSet))
            });

            var httpClient = _fixture.Create<HttpClient>();
            var sut = _fixture.Create<BPNAccess>();

            var result = await sut.FetchBusinessPartner("testpbn", "token");
            Assert.Equal(resultSet.bpn, result.First().bpn);
            Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        }

        [Fact]
        public async Task FetchBusinessPartner_Failure()
        {
            ConfigureHttpClientFactoryFixture(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var httpClient = _fixture.Create<HttpClient>();
            var sut = _fixture.Create<BPNAccess>();

            await Assert.ThrowsAsync<ServiceException>(async () => await sut.FetchBusinessPartner("testpbn", "token"));
            Assert.Equal("token", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        }
    }
}

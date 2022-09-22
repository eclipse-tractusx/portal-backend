using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Npgsql.Replication.PgOutput.Messages;
using Xunit;

namespace CatenaX.NetworkServices.Administration.Service.Tests.BusinessLogic;

public class ConnectorsSdFactoryServiceTests
{
    private readonly Guid _companyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");
    private readonly IFixture _fixture;
    private readonly ConnectorsSettings _settings;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IDocumentRepository _documentRepository;
    private readonly ConnectorsSdFactoryService _service;
    private readonly ICollection<Document> _documents;
    private readonly IHttpClientFactory _httpClientFactory;

    public ConnectorsSdFactoryServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _documents = new HashSet<Document>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _settings = new ConnectorsSettings
        {
            SdFactoryUrl = "https://www.api.sdfactory.com"
        };
        _httpClientFactory = A.Fake<IHttpClientFactory>();
        SetupRepositoryMethods();

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        
        _service = new ConnectorsSdFactoryService(Options.Create(_settings), _httpClientFactory, _portalRepositories);
    }

    [Fact]
    public async Task RegisterConnectorAsync_WithValidData_CreatesDocumentInDatabase()
    {
        // Arrange
        var contentJson = @"{
          'id': 'http://sdhub.int.demo.catena-x.net/selfdescription/vc/62a86c917ed7226dae676c86',
          '@context': [
            'https://www.w3.org/2018/credentials/v1',
            'https://abc.io/sd-document-v0.1.jsonld'
          ],
          'type': [
            'VerifiableCredential',
            'SD-document'
          ],
          'issuer': 'did:indy:idunion:test:JFcJRR9NSmtZaQGFMJuEjh',
          'issuanceDate': '2022-06-14T11:10:09Z',
          'expirationDate': '2022-09-12T11:10:09Z',
          'credentialSubject': {
            'bpn': 'BPNL000000000000',
            'company_number': '123456',
            'headquarter_country': 'DE',
            'legal_country': 'DE',
            'sd_type': 'connector',
            'service_provider': 'http://demo.test.com',
            'id': 'did:indy:idunion:test:123456789'
          },
          'proof': {
            'type': 'Ed25519Signature2018',
            'created': '2022-06-14T11:10:13Z',
            'proofPurpose': 'assertionMethod',
            'verificationMethod': 'did:indy:idunion:test:123456789#key-1',
            'jws': 'this-is-a-super-secret-secret-not'
          }
        }";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, FormContent(contentJson, "application/vc+ld+json"));
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var connectorInputModel = new ConnectorInputModel("Connec Tor", "https://connect-tor.com", ConnectorTypeId.COMPANY_CONNECTOR, ConnectorStatusId.ACTIVE, "de", Guid.NewGuid(), Guid.NewGuid());
        var accessToken = "this-is-a-super-secret-secret-not";
        var bpn = "BPNL000000000009";
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        await _service.RegisterConnectorAsync(connectorInputModel, accessToken, bpn, _companyUserId).ConfigureAwait(false);

        // Assert
        _documents.Should().HaveCount(1);
    }

    [Fact]
    public async Task  RegisterConnectorAsync_WithInvalidData_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(httpMessageHandlerMock);
        var connectorInputModel = new ConnectorInputModel("Connec Tor", "https://connect-tor.com", ConnectorTypeId.COMPANY_CONNECTOR, ConnectorStatusId.ACTIVE, "de", Guid.NewGuid(), Guid.NewGuid());
        var accessToken = "this-is-a-super-secret-secret-not";
        var bpn = "BPNL000000000009";
        A.CallTo(() => _httpClientFactory.CreateClient(A<string>._)).Returns(httpClient);

        // Act
        async Task Action() => await _service.RegisterConnectorAsync(connectorInputModel, accessToken, bpn, _companyUserId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(Action);
        exception.Message.Should().Be($"Access to SD factory failed with status code {HttpStatusCode.BadRequest}");
    }
    private static HttpContent FormContent(string s, string contentType)
    {
        HttpContent content = new StringContent(s);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return content;
    }

    private void SetupRepositoryMethods()
    { 
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<Action<Document>?>._))
            .Invokes(x =>
            {
                var documentName = x.Arguments.Get<string>("documentName")!;
                var documentContent = x.Arguments.Get<byte[]>("documentContent")!;
                var hash = x.Arguments.Get<byte[]>("hash")!;
                var setupOptionalFields = x.Arguments.Get<Action<Document>?>("setupOptionalFields");

                var document = new Document(Guid.NewGuid(), documentContent, hash, documentName, DateTimeOffset.UtcNow,
                    DocumentStatusId.PENDING);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            });
    }
}
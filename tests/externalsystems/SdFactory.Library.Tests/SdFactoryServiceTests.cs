﻿/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryServiceTests
{
    #region Initialization
    
    private static readonly IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers = new List<(UniqueIdentifierId Id, string Value)>
    {
        new (UniqueIdentifierId.VAT_ID, "JUSTATEST")
    };
    
    private readonly IPortalRepositories _portalRepositories;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICollection<Document> _documents;
    private readonly IOptions<SdFactorySettings> _options;
    private readonly ITokenService _tokenService;

    public SdFactoryServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _documents = new HashSet<Document>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _options = Options.Create(new SdFactorySettings
        {
            SdFactoryUrl = "https://www.api.sdfactory.com",
            SdFactoryIssuerBpn = "BPNL00000003CRHK"
        });
        _tokenService = A.Fake<ITokenService>();
        SetupRepositoryMethods();
    }

    #endregion
    
    #region Register Connector
    
    [Fact]
    public async Task RegisterConnectorAsync_WithValidData_CreatesDocumentInDatabase()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        const string contentJson = @"{
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, contentJson.ToFormContent("application/vc+ld+json"));
        CreateHttpClient(httpMessageHandlerMock);
        var service = new SdFactoryService(_portalRepositories, _tokenService, _options);

        // Act
        await service.RegisterConnectorAsync("https://connect-tor.com", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        document.DocumentName.Should().Be($"SelfDescription_Connector.json");
    }

    [Fact]
    public async Task  RegisterConnectorAsync_WithInvalidData_ThrowsException()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        CreateHttpClient(httpMessageHandlerMock);
        var service = new SdFactoryService(_portalRepositories, _tokenService, _options);

        // Act
        async Task Action() => await service.RegisterConnectorAsync("https://connect-tor.com", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(Action);
        exception.Message.Should().Be($"Access to SD factory failed with status code {HttpStatusCode.BadRequest}");
    }

    #endregion
    
    #region RegisterSelfDescription
    
    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithValidData_CreatesDocumentInDatabase()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        const string contentJson = @"{
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
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, contentJson.ToFormContent("application/vc+ld+json"));
        CreateHttpClient(httpMessageHandlerMock);
        var service = new SdFactoryService(_portalRepositories, _tokenService, _options);

        // Act
        await service.RegisterSelfDescriptionAsync(UniqueIdentifiers, "de", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        _documents.Should().HaveCount(1);
        var document = _documents.Single();
        document.DocumentName.Should().Be($"SelfDescription_LegalPerson.json");
    }

    [Fact]
    public async Task  RegisterSelfDescriptionAsync_WithInvalidData_ThrowsException()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        CreateHttpClient(httpMessageHandlerMock);
        var service = new SdFactoryService(_portalRepositories, _tokenService, _options);

        // Act
        async Task Action() => await service.RegisterSelfDescriptionAsync(UniqueIdentifiers, "de", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(Action);
        exception.Message.Should().Be($"Access to SD factory failed with status code {HttpStatusCode.BadRequest}");
    }

    #endregion
    
    #region Setup

    private void CreateHttpClient(HttpMessageHandler httpMessageHandlerMock)
    {
        var httpClient = new HttpClient(httpMessageHandlerMock) {BaseAddress = new Uri(_options.Value.SdFactoryUrl)};
        A.CallTo(() => _tokenService.GetAuthorizedClient<SdFactoryService>(_options.Value, CancellationToken.None))
            .ReturnsLazily(() => httpClient);
    }

    private void SetupRepositoryMethods()
    { 
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .Invokes(x =>
            {
                var documentName = x.Arguments.Get<string>("documentName")!;
                var documentContent = x.Arguments.Get<byte[]>("documentContent")!;
                var hash = x.Arguments.Get<byte[]>("hash")!;
                var documentTypeId = x.Arguments.Get<DocumentTypeId>("documentType")!;
                var setupOptionalFields = x.Arguments.Get<Action<Document>?>("setupOptionalFields");

                var document = new Document(Guid.NewGuid(), documentContent, hash, documentName, DateTimeOffset.UtcNow,
                    DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                _documents.Add(document);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }
    
    #endregion
}

/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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
    private readonly SdFactoryService _service;

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
        _service = new SdFactoryService(_tokenService, _options);
    }

    #endregion

    #region Register Connector

    [Fact]
    public async Task RegisterConnectorAsync_WithValidData_CreatesDocumentInDatabase()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        var id = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = CreateHttpClient(httpMessageHandlerMock);

        // Act
        await _service.RegisterConnectorAsync(id, "https://connect-tor.com", bpn, CancellationToken.None);

        // Assert
        _documents.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterConnectorAsync_WithInvalidData_ThrowsException()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string bpn = "BPNL000000000009";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = CreateHttpClient(httpMessageHandlerMock);

        // Act
        async Task Action() => await _service.RegisterConnectorAsync(id, "https://connect-tor.com", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(Action);
        exception.Message.Should().Be("call to external system sd-factory-connector-post failed with statuscode 400");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region RegisterSelfDescription

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithValidData_CreatesDocumentInDatabase()
    {
        // Arrange
        const string bpn = "BPNL000000000009";
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = CreateHttpClient(httpMessageHandlerMock);

        // Act
        await _service.RegisterSelfDescriptionAsync(applicationId, UniqueIdentifiers, "de", bpn, CancellationToken.None);

        // Assert
        _documents.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithInvalidData_ThrowsException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        const string bpn = "BPNL000000000009";
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = CreateHttpClient(httpMessageHandlerMock);

        // Act
        async Task Action() => await _service.RegisterSelfDescriptionAsync(applicationId, UniqueIdentifiers, "de", bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ServiceException>(Action);
        exception.Message.Should().Be($"call to external system sd-factory-selfdescription-post failed with statuscode 400");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Setup

    private HttpClient CreateHttpClient(HttpMessageHandler httpMessageHandlerMock)
    {
        var httpClient = new HttpClient(httpMessageHandlerMock) { BaseAddress = new Uri(_options.Value.SdFactoryUrl) };
        A.CallTo(() => _tokenService.GetAuthorizedClient<SdFactoryService>(_options.Value, CancellationToken.None))
            .Returns(httpClient);
        return httpClient;
    }

    private void SetupRepositoryMethods()
    {
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? action) =>
            {
                var document = new Document(Guid.NewGuid(), documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                action?.Invoke(document);
                _documents.Add(document);
            });

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}

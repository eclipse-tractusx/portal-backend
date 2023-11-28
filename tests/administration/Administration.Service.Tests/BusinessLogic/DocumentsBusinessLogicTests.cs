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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class DocumentsBusinessLogicTests
{
    private static readonly Guid ValidDocumentId = Guid.NewGuid();
    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly IDocumentRepository _documentRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOptions<DocumentSettings> _options;
    private readonly IIdentityService _identityService;

    public DocumentsBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _documentRepository = A.Fake<IDocumentRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _options = Options.Create(new DocumentSettings
        {
            EnableSeedEndpoint = true
        });
        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #region GetSeedData

    [Fact]
    public async Task GetSeedData_WithValidId_ReturnsValidData()
    {
        // Arrange
        SetupFakesForGetSeedData();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        var result = await sut.GetSeedData(ValidDocumentId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateConnectorAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        SetupFakesForGetSeedData();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        async Task Act() => await sut.GetSeedData(invalidId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"Document {invalidId} does not exists.");
    }

    [Fact]
    public async Task CreateConnectorAsync_WithCallFromTest_ThrowsForbiddenException()
    {
        // Arrange
        SetupFakesForGetSeedData();
        _options.Value.EnableSeedEndpoint = false;
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        async Task Act() => await sut.GetSeedData(ValidDocumentId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(Act);
        exception.Message.Should().Be("Endpoint can only be used on dev environment");
    }

    #endregion

    #region GetDocumentAsync

    [Fact]
    public async Task GetDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupFakesForGetDocument();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        var result = await sut.GetDocumentAsync(ValidDocumentId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GetDocumentAsync_WithNotExistingDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        SetupFakesForGetDocument();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        async Task Act() => await sut.GetDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Document {documentId} does not exist");
    }

    [Fact]
    public async Task GetDocumentAsync_WithWrongUser_ThrowsForbiddenException()
    {
        // Arrange
        var identity = _fixture.Create<IIdentityData>();
        A.CallTo(() => _identityService.IdentityData).Returns(identity);
        SetupFakesForGetDocument();
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        async Task Act() => await sut.GetDocumentAsync(ValidDocumentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("User is not allowed to access the document");
    }

    #endregion

    #region GetDocumentAsync

    [Fact]
    public async Task GetSelfDescriptionDocumentAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentDataByIdAndTypeAsync(ValidDocumentId, DocumentTypeId.SELF_DESCRIPTION))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId>(content, "test.json", MediaTypeId.JSON));
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        var result = await sut.GetSelfDescriptionDocumentAsync(ValidDocumentId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.json");
        result.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetSelfDescriptionDocumentAsync_WithNotExistingDocument_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentDataByIdAndTypeAsync(documentId, DocumentTypeId.SELF_DESCRIPTION))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId>());
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        // Act
        async Task Act() => await sut.GetSelfDescriptionDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Self description document {documentId} does not exist");
    }

    #endregion

    [Fact]
    public async Task GetFrameDocumentAsync_ReturnsExpectedResult()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>(content, "test.json", true, MediaTypeId.JSON));
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        //Act
        var result = await sut.GetFrameDocumentAsync(documentId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
        result.fileName.Should().Be("test.json");
    }

    [Fact]
    public async Task GetFrameDocumentAsync_WithInvalidDocumentTypeId_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>(content, "test.json", false, MediaTypeId.JSON));
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        //Act
        var Act = () => sut.GetFrameDocumentAsync(documentId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"document {documentId} does not exist.");
    }

    [Fact]
    public async Task GetFrameDocumentAsync_WithInvalidDocumentId_ThrowsNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        A.CallTo(() => _documentRepository.GetDocumentAsync(documentId, A<IEnumerable<DocumentTypeId>>._))
            .ReturnsLazily(() => new ValueTuple<byte[], string, bool, MediaTypeId>());
        var sut = new DocumentsBusinessLogic(_portalRepositories, _identityService, _options);

        //Act
        var Act = () => sut.GetFrameDocumentAsync(documentId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"document {documentId} does not exist.");
    }

    #region Setup

    private void SetupFakesForGetSeedData()
    {
        A.CallTo(() => _documentRepository.GetDocumentSeedDataByIdAsync(A<Guid>.That.Matches(x => x == ValidDocumentId)))
            .Returns(_fixture.Create<DocumentSeedData>());
        A.CallTo(() => _documentRepository.GetDocumentSeedDataByIdAsync(A<Guid>.That.Not.Matches(x => x == ValidDocumentId)))
            .Returns((DocumentSeedData?)null);
    }

    private void SetupFakesForGetDocument()
    {
        var content = new byte[7];
        A.CallTo(() => _documentRepository.GetDocumentDataAndIsCompanyUserAsync(ValidDocumentId, _identity.CompanyId))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool>(content, "test.pdf", MediaTypeId.PDF, true));
        A.CallTo(() => _documentRepository.GetDocumentDataAndIsCompanyUserAsync(A<Guid>.That.Not.Matches(x => x == ValidDocumentId), _identity.CompanyId))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool>());
        A.CallTo(() => _documentRepository.GetDocumentDataAndIsCompanyUserAsync(ValidDocumentId, A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .ReturnsLazily(() => new ValueTuple<byte[], string, MediaTypeId, bool>(content, "test.pdf", MediaTypeId.PDF, false));
    }

    #endregion
}

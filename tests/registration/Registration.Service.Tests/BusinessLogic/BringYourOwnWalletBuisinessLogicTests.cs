/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library;
using Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library.Models;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.BusinessLogic;

public class BringYourOwnWalletBuisinessLogicTests
{
    private readonly IUniversalDidResolverService _universalDidResolverService;
    private readonly IBringYourOwnWalletBusinessLogic _sut;
    private readonly IPortalRepositories _portalRepositories = A.Fake<IPortalRepositories>();
    public BringYourOwnWalletBuisinessLogicTests()
    {
        _universalDidResolverService = A.Fake<IUniversalDidResolverService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _sut = new BringYourOwnWalletBusinessLogic(_universalDidResolverService, _portalRepositories);
    }

    [Fact]
    public async Task ValidateDid_Completes_WhenDidIsValid()
    {
        // Arrange
        const string did = "did:web:123";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);

        // Act & Assert
        await _sut.Invoking(s => s.ValidateDid(did, CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateDid_ThrowsSericeException_WhenDidHasError()
    {
        // Arrange
        const string did = "did:web:123";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata("notFound"), didDocument);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));

        // Act
        var act = () => _sut.ValidateDid(did, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("DID validation failed. DID Document is not valid.");
    }

    [Fact]
    public async Task ValidateDid_ThrowsConflictException_WhenDidExists()
    {
        // Arrange
        const string did = "did:web:123";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(companyRepository);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => companyRepository.IsDidInUse(did)).ReturnsLazily(() => true);

        // Act
        var act = () => _sut.ValidateDid(did, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("DID is already in use.");
    }

    [Fact]
    public async Task ValidateDid_ThrowsServiceException_WhenSchemaIsInvalid()
    {
        // Arrange
        const string did = "did:web:123";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);

        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => false);

        // Act
        var act = () => _sut.ValidateDid(did, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("DID validation failed. DID Document is not valid.");
    }

    [Fact]
    public async Task ValidateDid_ThrowsServiceException_WhenValidationResultIsNull()
    {
        // Arrange
        const string did = "did:web:empty";
        var didDocument = JsonDocument.Parse("{}");
        var validationResult = new DidValidationResult(
                    new DidResolutionMetadata(Error: null),
                    didDocument
                );
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(_ => Task.FromResult<DidValidationResult>(validationResult));

        // Act
        var act = () => _sut.ValidateDid(did, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("DID validation failed. DID Document is not valid.");
    }

    [Fact]
    public async Task SaveCustomerWalletAsync_SavesWallet_WhenDidIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "did:web:example.com";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:example.com\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .DoesNothing();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        // Act
        await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SaveCustomerWalletAsync_ThrowsException_WhenDidIsEmpty()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:example.com\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        // Act
        Func<Task> act = async () => await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Invalid DID. DID cannot be empty or NULL.");
    }

    [Fact]
    public async Task SaveCustomerWalletAsync_ThrowsException_WhenDidIsInvalid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "did:web:INVALID";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:example.com\"}");

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(A.Fake<ICompanyRepository>());
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._)).Throws<Exception>();
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        // Act
        Func<Task> act = async () => await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SaveCustomerWalletAsync_ThrowsException_WhenCompanyIdInvalid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "did:web:example.com";
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web:example.com\"}");
        var companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._)).Throws<Exception>();
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => false);
        // Act
        Func<Task> act = async () => await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Company ID not found or not valid.");
    }

    [Fact]
    public async Task GetCompanyWalletDidAsync_ReturnsDid_WhenWalletExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var expectedDid = "did:web:example.com";
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.GetCompanyHolderDidAsync(companyId))
            .Returns(Task.FromResult<string?>(expectedDid));
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);

        // Act
        var result = await _sut.getCompanyWalletDidAsync(companyId);

        // Assert
        result.Should().Be(expectedDid);
    }

    [Fact]
    public async Task GetCompanyWalletDidAsync_ThrowException_WhenWalletDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        A.CallTo(() => companyRepository.GetCompanyHolderDidAsync(companyId))
            .Returns(Task.FromResult<string?>(null));
        // Act
        Func<Task> act = async () => await _sut.getCompanyWalletDidAsync(companyId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Company wallet DID not found.");
    }

    [Fact]
    public async Task GetCompanyWalletDidAsync_ThrowException_WhenCompanyIdDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => false);
        A.CallTo(() => companyRepository.GetCompanyHolderDidAsync(companyId))
            .Returns(Task.FromResult<string?>(null));
        // Act
        Func<Task> act = async () => await _sut.getCompanyWalletDidAsync(companyId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Company ID not found or not valid.");
    }

    [Fact]
    public async Task SaveCustomerWallet_InvalidDocumentId_ThrowException()
    {
        // Arrange
        const string did = "did:web:123";
        var companyId = Guid.NewGuid();
        var didDocument = JsonDocument.Parse("{}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .DoesNothing();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);

        // Act

        Func<Task> act = async () =>
            await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("DID validation failed: missing 'id' property.");
    }

    [Fact]
    public async Task SaveCustomerWallet_InvalidDidFormat_ThrowException()
    {
        // Arrange
        const string did = "did:web:123";
        var companyId = Guid.NewGuid();
        var didDocument = JsonDocument.Parse("{\"id\":\":web:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .DoesNothing();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);

        // Act

        Func<Task> act = async () =>
            await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("Invalid DID format: must start with 'did:'.");
    }

    [Fact]
    public async Task SaveCustomerWallet_InvalidDidForm_ThrowException()
    {
        // Arrange
        const string did = "did:web:123";
        var companyId = Guid.NewGuid();
        var didDocument = JsonDocument.Parse("{\"id\":\"did:web\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .DoesNothing();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);

        // Act

        Func<Task> act = async () =>
            await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("Invalid DID format: must be in the form 'did:<method>:<identifier>'.");
    }

    [Fact]
    public async Task SaveCustomerWallet_InvalidDidMethod_ThrowException()
    {
        // Arrange
        const string did = "did:web:123";
        var companyId = Guid.NewGuid();
        var didDocument = JsonDocument.Parse("{\"id\":\"did:error:123\"}");
        var validationResult = new DidValidationResult(new DidResolutionMetadata(null), didDocument);
        var companyRepository = A.Fake<ICompanyRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>().CreateCustomerWallet(companyId, did, didDocument))
            .DoesNothing();
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(companyRepository);
        A.CallTo(() => companyRepository.IsExistingCompany(companyId)).ReturnsLazily(() => true);
        A.CallTo(() => _universalDidResolverService.ValidateDid(did, A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(validationResult));
        A.CallTo(() => _universalDidResolverService.ValidateSchema(didDocument, A<CancellationToken>._))
            .ReturnsLazily(() => true);

        // Act

        Func<Task> act = async () =>
            await _sut.SaveCustomerWalletAsync(companyId, did);

        // Assert
        await act.Should().ThrowAsync<ControllerArgumentException>()
            .WithMessage("Unsupported DID method: 'error'. Only 'did:web' is supported.");
    }
}

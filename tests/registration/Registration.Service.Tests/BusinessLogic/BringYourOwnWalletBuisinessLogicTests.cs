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

    public BringYourOwnWalletBuisinessLogicTests()
    {
        _universalDidResolverService = A.Fake<IUniversalDidResolverService>();
        _sut = new BringYourOwnWalletBusinessLogic(_universalDidResolverService);
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
        await act.Should().ThrowAsync<UnsupportedMediaTypeException>()
            .WithMessage("DID validation failed. DID Document is not valid.");
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
        await act.Should().ThrowAsync<UnsupportedMediaTypeException>()
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
        await act.Should().ThrowAsync<UnsupportedMediaTypeException>()
            .WithMessage("DID validation failed. DID Document is not valid.");
    }
}

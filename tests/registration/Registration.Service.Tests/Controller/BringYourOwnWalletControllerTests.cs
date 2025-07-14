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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests;

public class BringYourOwnWalletControllerTests
{
    private readonly IIdentityData _identity;
    private readonly BringYourOwnWalletController _controller;
    private readonly IBringYourOwnWalletBusinessLogic _bringYourOwnWalletBusinessLogicFake;

    public BringYourOwnWalletControllerTests()
    {
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _bringYourOwnWalletBusinessLogicFake = A.Fake<IBringYourOwnWalletBusinessLogic>();
        _controller = new BringYourOwnWalletController(_bringYourOwnWalletBusinessLogicFake);
        _controller.AddControllerContextWithClaimAndBearer("ac-token", _identity);
    }

    [Fact]
    public async Task ValidateDid_ShouldBeValid()
    {
        // Arrange
        var did = "did:web:example.com";
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.ValidateDid(did, A<CancellationToken>._))
            .Returns(Task.FromResult(System.Text.Json.JsonDocument.Parse("{}")));

        // Act
        await _controller.validateDid(did, CancellationToken.None);

        // Assert
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.ValidateDid(did, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ValidateDid_WhenDidIsInvalid_ShouldThrowServiceException()
    {
        // Arrange
        var did = "did:web:invalid";
        var exception = new ServiceException("DID validation failed", HttpStatusCode.BadRequest);
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.ValidateDid(did, A<CancellationToken>._)).Throws(exception);

        // Act
        Func<Task> act = async () => await _controller.validateDid(did, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ServiceException>()
            .WithMessage("DID validation failed");
    }

    [Fact]
    public async Task SaveHolderDid_WhenDidIsInvalid_ShouldThrowServiceException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "did:web:example.com";
        var exception = new ServiceException("DID validation failed", HttpStatusCode.BadRequest);

        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.SaveCustomerWalletAsync(companyId, did))
            .Throws(exception);

        // Act
        Func<Task> act = async () => await _controller.SaveHolderDid(companyId, did);

        // Assert
        await act.Should().ThrowAsync<ServiceException>()
            .WithMessage("DID validation failed");
    }

    [Fact]
    public async Task SaveHolderDid_ShouldBeValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var did = "did:web:example.com";
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.ValidateDid(did, A<CancellationToken>._))
            .Returns(Task.FromResult(System.Text.Json.JsonDocument.Parse("{}")));

        // Act
        await _controller.SaveHolderDid(companyId, did);

        // Assert
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.SaveCustomerWalletAsync(companyId, did)).MustHaveHappenedOnceExactly();
    }
    [Fact]
    public async Task GetHolderDid_ShouldReturnDid_WhenDidExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var expectedDid = "did:web:example.com";
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.getCompanyWalletDidAsync(companyId))
            .Returns(Task.FromResult(expectedDid));

        // Act
        var result = await _controller.GetHolderDid(companyId);

        // Assert
        result.Should().Be(expectedDid);
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.getCompanyWalletDidAsync(companyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetHolderDid_ShouldReturnNull_WhenDidDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        A.CallTo(() => _bringYourOwnWalletBusinessLogicFake.getCompanyWalletDidAsync(companyId))
        .Throws(new NotFoundException("Company wallet DID not found for the given company ID."));

        // Act
        Func<Task> act = async () => await _controller.GetHolderDid(companyId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Company wallet DID not found for the given company ID.");
    }
}

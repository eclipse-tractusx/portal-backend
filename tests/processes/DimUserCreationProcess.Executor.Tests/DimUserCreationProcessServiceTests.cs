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

using Org.Eclipse.TractusX.Portal.Backend.Dim.Library;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.DimUserCreationProcess.Executor;

namespace Org.Eclipse.TractusX.Portal.Backend.DimUserCreationProcess.Executor.Tests;

public class DimUserCreationProcessServiceTests
{
    private const string Bpn = "BPNL00000001TEST";
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IDimService _dimService;
    private readonly IDimUserCreationProcessService _sut;

    public DimUserCreationProcessServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepositories = A.Fake<IPortalRepositories>();
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();

        _dimService = A.Fake<IDimService>();

        A.CallTo(() => portalRepositories.GetInstance<IServiceAccountRepository>())
            .Returns(_serviceAccountRepository);

        _sut = new DimUserCreationProcessService(_dimService, portalRepositories);
    }

    #region CreateDimUser

    [Fact]
    public async Task CreateDimUser_WithValid_ReturnsExpected()
    {
        // Arrange
        var dimServiceAccountId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var expectedServiceAccountName = "dim-sa-test";
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(A<Guid>._))
            .Returns((true, Bpn, "sa-test"));

        // Act
        var result = await _sut.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None);

        // Act
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(dimServiceAccountId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dimService.CreateTechnicalUser(Bpn, A<TechnicalUserData>.That.Matches(x => x.ExternalId == processId && x.Name == expectedServiceAccountName), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE);
    }

    [Fact]
    public async Task CreateDimUser_WithInvalidDimServiceAccountId_ThrowsNotFoundException()
    {
        // Arrange
        var dimServiceAccountId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(A<Guid>._))
            .Returns(default((bool, string?, string?)));
        Task Act() => _sut.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Act
        ex.Message.Should().Be($"DimServiceAccountId {dimServiceAccountId} does not exist");
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(dimServiceAccountId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dimService.CreateTechnicalUser(Bpn, A<TechnicalUserData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateDimUser_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        var dimServiceAccountId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(A<Guid>._))
            .Returns((true, null, null));
        Task Act() => _sut.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Act
        ex.Message.Should().Be("Bpn must not be null");
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(dimServiceAccountId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dimService.CreateTechnicalUser(Bpn, A<TechnicalUserData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateDimUser_WithValidMissingServiceAccountName_ThrowsConflictException()
    {
        // Arrange
        var dimServiceAccountId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(A<Guid>._))
            .Returns((true, Bpn, null));
        Task Act() => _sut.CreateDimUser(processId, dimServiceAccountId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Act
        ex.Message.Should().Be("Service Account Name must not be null");
        A.CallTo(() => _serviceAccountRepository.GetDimServiceAccountData(dimServiceAccountId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dimService.CreateTechnicalUser(Bpn, A<TechnicalUserData>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #endregion
}

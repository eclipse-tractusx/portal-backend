/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.SelfDescriptionCreation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.SelfDescriptionCreation.Executor.Tests;

public class SdCreationProcessTypeExecutorTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly SdCreationProcessTypeExecutor _executor;
    private readonly IFixture _fixture;
    private readonly IEnumerable<ProcessStepTypeId> _executableSteps;
    private readonly ISdFactoryService _sdFactoryService;

    public SdCreationProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _sdFactoryService = A.Fake<ISdFactoryService>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();

        A.CallTo(() => portalRepositories.GetInstance<IConnectorsRepository>())
            .Returns(_connectorsRepository);
        A.CallTo(() => portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);

        var settings = new SelfDescriptionProcessSettings
        {
            SelfDescriptionDocumentUrl = "https://selfdescription.url"
        };

        _executor = new SdCreationProcessTypeExecutor(portalRepositories, _sdFactoryService, Options.Create(settings));

        _executableSteps = new[] { ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION };
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExisting_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert
        result.Should().NotBeNull();
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_ForCompanies_ReturnsExpected()
    {
        // Act initialize
        var processId = Guid.NewGuid();
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        var company = new ValueTuple<Guid, IEnumerable<(UniqueIdentifierId Id, string Value)>, string?, string>(
            Guid.NewGuid(),
            new List<(UniqueIdentifierId Id, string Value)> { new(UniqueIdentifierId.VAT_ID, "test") },
            "BPNL000000001TEST",
            "DE");
        A.CallTo(() => _companyRepository.GetCompanyByProcessId(processId))
            .Returns(company);

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);

        A.CallTo(() => _sdFactoryService.RegisterSelfDescriptionAsync(company.Item1, A<IEnumerable<(UniqueIdentifierId Id, string Value)>>._, A<string>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteProcessStep_ForConnectors_ReturnsExpected()
    {
        // Act initialize
        var processId = Guid.NewGuid();
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        var connector = new ValueTuple<Guid, string, Guid>(
            Guid.NewGuid(),
            "BPNL000000001TEST",
            Guid.NewGuid());
        A.CallTo(() => _connectorsRepository.GetConnectorForProcessId(processId))
            .Returns(connector);

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);

        A.CallTo(() => _sdFactoryService.RegisterConnectorAsync(connector.Item1, A<string>._, "BPNL000000001TEST", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingTestException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var company = new ValueTuple<Guid, IEnumerable<(UniqueIdentifierId Id, string Value)>, string?, string>(
            Guid.NewGuid(),
            new List<(UniqueIdentifierId Id, string Value)> { new(UniqueIdentifierId.VAT_ID, "test") },
            "BPNL000000001TEST",
            "DE");
        A.CallTo(() => _companyRepository.GetCompanyByProcessId(processId))
            .Returns(company);

        var error = _fixture.Create<TestException>();
        A.CallTo(() => _sdFactoryService.RegisterSelfDescriptionAsync(A<Guid>._, A<IEnumerable<(UniqueIdentifierId Id, string Value)>>._, A<string>._, A<string>._, A<CancellationToken>._))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert execute
        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        executionResult.ScheduleStepTypeIds.Should().ContainInOrder(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_COMPANY_CREATION);
        executionResult.SkipStepTypeIds.Should().BeNull();
        executionResult.ProcessMessage.Should().Be(error.Message);
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingSystemException_Throws()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>());

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        var company = new ValueTuple<Guid, IEnumerable<(UniqueIdentifierId Id, string Value)>, string?, string>(
            Guid.NewGuid(),
            new List<(UniqueIdentifierId Id, string Value)> { new(UniqueIdentifierId.VAT_ID, "test") },
            "BPNL000000001TEST",
            "DE");
        A.CallTo(() => _companyRepository.GetCompanyByProcessId(processId))
            .Returns(company);

        var error = new SystemException(_fixture.Create<string>());
        A.CallTo(() => _sdFactoryService.RegisterSelfDescriptionAsync(A<Guid>._, A<IEnumerable<(UniqueIdentifierId Id, string Value)>>._, A<string>._, A<string>._, A<CancellationToken>._))
            .Throws(error);

        // Act execute
        async Task Act() => await _executor.ExecuteProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);
        var ex = await Assert.ThrowsAsync<SystemException>(Act);

        // Assert execute
        ex.Message.Should().Be(error.Message);
    }

    #endregion

    #region GetProcessTypeId

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Act
        var result = _executor.GetProcessTypeId();

        // Assert
        result.Should().Be(ProcessTypeId.SELF_DESCRIPTION_CREATION);
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, true)]
    [InlineData(ProcessStepTypeId.SELF_DESCRIPTION_CONNECTOR_CREATION, true)]
    [InlineData(ProcessStepTypeId.AWAIT_START_AUTOSETUP, false)]
    public void IsExecutableProcessStep_ReturnsExpected(ProcessStepTypeId processStepTypeId, bool expectedValue)
    {
        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region IsLockRequested

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _executor.IsLockRequested(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION);

        // Assert
        result.Should().Be(false);
    }

    #endregion

    #region GetExecutableStepTypeIds

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        //Act
        var result = _executor.GetExecutableStepTypeIds();

        // Assert
        result.Should().HaveCount(_executableSteps.Count())
            .And.BeEquivalentTo(_executableSteps);
    }

    #endregion
}

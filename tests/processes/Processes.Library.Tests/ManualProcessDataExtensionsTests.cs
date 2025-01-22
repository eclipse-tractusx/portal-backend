/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Library.Tests;

public class ManualProcessDataExtensionsTests
{
    private readonly IProcessRepositories _processRepositories;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly string _entityName;
    private readonly Func<string> _getProcessEntityName;
    private readonly IFixture _fixture;

    public ManualProcessDataExtensionsTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _processRepositories = A.Fake<IProcessRepositories>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _processRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>())
            .Returns(_processStepRepository);

        _entityName = _fixture.Create<string>();
        _getProcessEntityName = () => _entityName;
    }

    #region CreateManualProcessData

    [Fact]
    public void CreateManualProcessData_ReturnsExpected()
    {
        // Arrange
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = null };
        var processSteps = _fixture.CreateMany<(Guid ProcessStepId, DateTimeOffset Now)>(5).Select(x => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(x.ProcessStepId, _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, x.Now)).ToImmutableArray();
        var stepTypeId = processSteps[2].ProcessStepTypeId;

        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessSteps, processSteps)
            .Create();

        // Act
        var result = sut.CreateManualProcessData(stepTypeId, _processRepositories, _getProcessEntityName);

        // Assert
        result.Should().NotBeNull().And.BeOfType<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>().And.Match<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>(
            data =>
                data.ProcessStepTypeId == stepTypeId &&
                data.Process == sut.Process &&
                data.ProcessSteps.SequenceEqual(sut.ProcessSteps!) &&
                data.ProcessRepositories == _processRepositories);
    }

    [Fact]
    public void CreateManualProcessData_WithNullVerifyProcessData_Throws()
    {
        // Arrange
        var sut = default(VerifyProcessData<ProcessTypeId, ProcessStepTypeId>?);

        var Act = () => sut.CreateManualProcessData(_fixture.Create<ProcessStepTypeId>(), _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<NotFoundException>(Act);

        // Assert
        result.Message.Should().Be($"{_entityName} does not exist");
    }

    [Fact]
    public void CreateManualProcessData_WithNullProcess_Throws()
    {
        // Arrange
        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, default(Process<ProcessTypeId, ProcessStepTypeId>?))
            .Create();

        var Act = () => sut.CreateManualProcessData(_fixture.Create<ProcessStepTypeId>(), _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<ConflictException>(Act);

        // Assert
        result.Message.Should().Be($"{_entityName} is not associated with any process");
    }

    [Fact]
    public void CreateManualProcessData_WithLockedProcess_Throws()
    {
        // Arrange
        var expiryDate = _fixture.Create<DateTimeOffset>();
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = expiryDate };
        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .Create();

        var Act = () => sut.CreateManualProcessData(_fixture.Create<ProcessStepTypeId>(), _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<ConflictException>(Act);

        // Assert
        result.Message.Should().Be($"process {process.Id} associated with {_entityName} is locked, lock expiry is set to {expiryDate}");
    }

    [Fact]
    public void CreateManualProcessData_WithNullProcessSteps_Throws()
    {
        // Arrange
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = null };

        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessSteps, default(IEnumerable<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?))
            .Create();

        var Act = () => sut.CreateManualProcessData(_fixture.Create<ProcessStepTypeId>(), _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("processSteps should never be null here");
    }

    [Fact]
    public void CreateManualProcessData_WithInvalidProcessStepStatus_Throws()
    {
        // Arrange
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = null };
        var processSteps = _fixture.CreateMany<(Guid ProcessStepId, DateTimeOffset Now)>(5).Select(x => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(x.ProcessStepId, _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.DONE, process.Id, x.Now)).ToImmutableArray();

        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessSteps, processSteps)
            .Create();

        var Act = () => sut.CreateManualProcessData(_fixture.Create<ProcessStepTypeId>(), _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("processSteps should never have any other status than TODO here");
    }

    [Fact]
    public void CreateManualProcessData_WithInvalidProcessStepType_Throws()
    {
        // Arrange
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = null };
        var processSteps = _fixture.CreateMany<(Guid ProcessStepId, DateTimeOffset Now)>(5).Select(x => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(x.ProcessStepId, _fixture.Create<ProcessStepTypeId>(), ProcessStepStatusId.TODO, process.Id, x.Now)).ToImmutableArray();
        var stepTypeId = Enum.GetValues<ProcessStepTypeId>().Except(processSteps.Select(step => step.ProcessStepTypeId)).First();

        var sut = _fixture.Build<VerifyProcessData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessSteps, processSteps)
            .Create();

        var Act = () => sut.CreateManualProcessData(stepTypeId, _processRepositories, _getProcessEntityName);

        // Act
        var result = Assert.Throws<ConflictException>(Act);

        // Assert
        result.Message.Should().Be($"{_entityName}, process step {stepTypeId} is not eligible to run");
    }

    #endregion

    #region RequestLock

    [Fact]
    public void RequestLock_WithUnLockedProcess_ReturnsExpected()
    {
        // Arrange
        var expiryDate = _fixture.Create<DateTimeOffset>();
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = null };
        var sut = _fixture.Build<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessRepositories, _processRepositories)
            .Create();

        // Act
        sut.RequestLock(expiryDate);

        // Assert
        sut.Process.LockExpiryDate.Should().Be(expiryDate);
        A.CallTo(() => _processRepositories.Attach(process, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void RequestLock_WithLockedProcess_Throws()
    {
        // Arrange
        var expiryDate = _fixture.Create<DateTimeOffset>();
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), _fixture.Create<ProcessTypeId>(), Guid.NewGuid()) { LockExpiryDate = expiryDate };
        var sut = _fixture.Build<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Process, process)
            .With(x => x.ProcessRepositories, _processRepositories)
            .Create();

        var Act = () => sut.RequestLock(DateTimeOffset.UtcNow);
        // Act
        var result = Assert.Throws<UnexpectedConditionException>(Act);

        // Assert
        result.Message.Should().Be("process TryLock should never fail here");
        sut.Process.LockExpiryDate.Should().Be(expiryDate);
        A.CallTo(() => _processRepositories.Attach(process, null)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SkipProcessSteps

    [Fact]
    public void SkipProcessSteps_ReturnsExpected()
    {
        // Arrange
        var process = _fixture.Create<Process<ProcessTypeId, ProcessStepTypeId>>();
        var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(4).ToArray();
        var before = DateTimeOffset.UtcNow.AddDays(-1);
        var processSteps0 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps1 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps2 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps3 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before)
            };

        var processSteps = new[]
            {
                processSteps0[0],
                processSteps1[0],
                processSteps2[0],
                processSteps3[0],
                processSteps0[1],
                processSteps1[1],
                processSteps2[1],
                processSteps3[1],
                processSteps0[2],
                processSteps1[2],
                processSteps2[2],
                processSteps3[2],
            };

        var modifiedProcessSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>? Initialize, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>> Modify)> processStepIdInitializeModify) =>
            {
                foreach (var (stepId, initialize, modify) in processStepIdInitializeModify)
                {
                    var step = new ProcessStep<ProcessTypeId, ProcessStepTypeId>(stepId, default, default, Guid.Empty, default);
                    initialize?.Invoke(step);
                    modify(step);
                    modifiedProcessSteps.Add(step);
                }
            });

        var sut = _fixture.Build<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.ProcessStepTypeId, stepTypeIds[3])
            .With(x => x.Process, process)
            .With(x => x.ProcessRepositories, _processRepositories)
            .With(x => x.ProcessSteps, processSteps)
            .Create();

        // Act
        sut.SkipProcessSteps(new[] { stepTypeIds[1], stepTypeIds[2], stepTypeIds[3] });

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedProcessSteps.Should().HaveCount(6).And.Satisfy(
            x => processSteps1.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED && x.DateLastChanged != before,
            x => processSteps1.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before,
            x => processSteps1.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before,
            x => processSteps2.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED && x.DateLastChanged != before,
            x => processSteps2.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before,
            x => processSteps2.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before
        );
    }

    #endregion

    #region SkipProcessStepsExcept

    [Fact]
    public void SkipProcessStepsExcept_ReturnsExpected()
    {
        // Arrange
        var process = _fixture.Create<Process<ProcessTypeId, ProcessStepTypeId>>();
        var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(4).ToArray();
        var before = DateTimeOffset.UtcNow.AddDays(-1);
        var processSteps0 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps1 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps2 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before)
            };
        var processSteps3 = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
            {
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before),
                new(Guid.NewGuid(), stepTypeIds[3], ProcessStepStatusId.TODO, process.Id, before)
            };

        var processSteps = new[]
            {
                processSteps0[0],
                processSteps1[0],
                processSteps2[0],
                processSteps3[0],
                processSteps0[1],
                processSteps1[1],
                processSteps2[1],
                processSteps3[1],
                processSteps0[2],
                processSteps1[2],
                processSteps2[2],
                processSteps3[2],
            };

        var modifiedProcessSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>? Initialize, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>> Modify)> processStepIdInitializeModify) =>
            {
                foreach (var (stepId, initialize, modify) in processStepIdInitializeModify)
                {
                    var step = new ProcessStep<ProcessTypeId, ProcessStepTypeId>(stepId, default, default, Guid.Empty, default);
                    initialize?.Invoke(step);
                    modify(step);
                    modifiedProcessSteps.Add(step);
                }
            });

        var sut = _fixture.Build<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.ProcessStepTypeId, stepTypeIds[3])
            .With(x => x.Process, process)
            .With(x => x.ProcessRepositories, _processRepositories)
            .With(x => x.ProcessSteps, processSteps)
            .Create();

        // Act
        sut.SkipProcessStepsExcept(new[] { stepTypeIds[1], stepTypeIds[2] });

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .MustHaveHappenedOnceExactly();

        modifiedProcessSteps.Should().HaveCount(3).And.Satisfy(
            x => processSteps0.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.SKIPPED && x.DateLastChanged != before,
            x => processSteps0.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before,
            x => processSteps0.Any(step => step.Id == x.Id) && x.ProcessStepStatusId == ProcessStepStatusId.DUPLICATE && x.DateLastChanged != before
        );
    }

    #endregion

    #region ScheduleProcessSteps

    [Fact]
    public void ScheduleProcessSteps_ReturnsExpected()
    {
        // Arrange
        var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3);
        var now = DateTimeOffset.UtcNow;

        var createdSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .ReturnsLazily((IEnumerable<(ProcessStepTypeId ProcesssStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatusProcessIds) =>
            {
                createdSteps.AddRange(processStepTypeStatusProcessIds.Select(data => new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), data.ProcesssStepTypeId, data.ProcessStepStatusId, data.ProcessId, now)));
                return createdSteps;
            });

        var sut = _fixture.Build<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.ProcessRepositories, _processRepositories)
            .Create();

        // Act
        sut.ScheduleProcessSteps(stepTypeIds);

        // Assert
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId, ProcessStepStatusId, Guid)>>._))
            .MustHaveHappenedOnceExactly();
        createdSteps.Should().HaveCount(3).And.Satisfy(
            x => x.ProcessStepTypeId == stepTypeIds.ElementAt(0) && x.ProcessId == sut.Process.Id && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
            x => x.ProcessStepTypeId == stepTypeIds.ElementAt(1) && x.ProcessId == sut.Process.Id && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
            x => x.ProcessStepTypeId == stepTypeIds.ElementAt(2) && x.ProcessId == sut.Process.Id && x.ProcessStepStatusId == ProcessStepStatusId.TODO
        );
    }

    #endregion

    #region FinalizeProcessStep

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FinalizeProcessStep_ReturnsExpected(bool locked)
    {
        // Arrange
        var version = Guid.NewGuid();
        var process = _fixture.Build<Process<ProcessTypeId, ProcessStepTypeId>>()
            .With(x => x.Version, version)
            .With(x => x.LockExpiryDate, locked ? DateTimeOffset.UtcNow : default(DateTimeOffset?))
            .Create();
        var stepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3).ToArray();
        var before = DateTimeOffset.UtcNow.AddDays(-1);
        var processSteps = new ProcessStep<ProcessTypeId, ProcessStepTypeId>[]
        {
            new(Guid.NewGuid(), stepTypeIds[0], ProcessStepStatusId.TODO, process.Id, before),
            new(Guid.NewGuid(), stepTypeIds[1], ProcessStepStatusId.TODO, process.Id, before),
            new(Guid.NewGuid(), stepTypeIds[2], ProcessStepStatusId.TODO, process.Id, before)
        };

        var modifiedProcessSteps = new List<ProcessStep<ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>? Initialize, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>> Modify)> processStepIdInitializeModify) =>
            {
                foreach (var (stepId, initialize, modify) in processStepIdInitializeModify)
                {
                    var step = new ProcessStep<ProcessTypeId, ProcessStepTypeId>(stepId, default, default, Guid.Empty, default);
                    initialize?.Invoke(step);
                    modify(step);
                    modifiedProcessSteps.Add(step);
                }
            });

        var sut = new ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>(stepTypeIds[1], process, processSteps, _processRepositories);

        // Act
        sut.FinalizeProcessStep<ProcessTypeId, ProcessStepTypeId>();

        // Assert
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>?, Action<ProcessStep<ProcessTypeId, ProcessStepTypeId>>)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _processRepositories.Attach(process, null))
            .MustHaveHappenedOnceExactly();

        process.LockExpiryDate.Should().BeNull();
        process.Version.Should().NotBe(version);
        modifiedProcessSteps.Should().ContainSingle().Which.Should().Match<ProcessStep<ProcessTypeId, ProcessStepTypeId>>(
            x => x.Id == processSteps[1].Id && x.ProcessStepStatusId == ProcessStepStatusId.DONE && x.DateLastChanged != before
        );
    }

    #endregion
}

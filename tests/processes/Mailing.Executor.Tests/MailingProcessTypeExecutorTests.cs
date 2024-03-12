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

using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Executor.Tests;

public class MailingProcessTypeExecutorTests
{
    private readonly IMailingInformationRepository _mailingInformationRepository;
    private readonly MailingProcessTypeExecutor _executor;
    private readonly IFixture _fixture;
    private readonly IEnumerable<ProcessStepTypeId> _executableSteps;
    private readonly IMailingService _mailingService;

    public MailingProcessTypeExecutorTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mailingService = A.Fake<IMailingService>();
        var portalRepositories = A.Fake<IPortalRepositories>();
        _mailingInformationRepository = A.Fake<IMailingInformationRepository>();

        A.CallTo(() => portalRepositories.GetInstance<IMailingInformationRepository>())
            .Returns(_mailingInformationRepository);

        _executor = new MailingProcessTypeExecutor(portalRepositories, _mailingService);

        _executableSteps = Enumerable.Repeat(ProcessStepTypeId.SEND_MAIL, 1);
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExisting_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();

        // Act
        var result = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_ReturnsExpected()
    {
        // Act initialize
        var processId = Guid.NewGuid();
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        var mailingId1 = Guid.NewGuid();
        var mailingId2 = Guid.NewGuid();
        var mailing1 = new MailingInformation(mailingId1, processId, "test@mail.de", "test-template", new Dictionary<string, string>(), MailingStatusId.PENDING);
        var mailing2 = new MailingInformation(mailingId2, processId, "other@mail.de", "test-template", new Dictionary<string, string>(), MailingStatusId.PENDING);
        SetupFakes(processId, mailing1, mailing2);

        // Act
        var result = await _executor.ExecuteProcessStep(ProcessStepTypeId.SEND_MAIL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.SEND_MAIL);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().Be("send mail to test@mail.de");

        A.CallTo(() => _mailingService.SendMails(mailing1.Email, A<IReadOnlyDictionary<string, string>>._, mailing1.Template))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails("other@email.com", A<IReadOnlyDictionary<string, string>>._, "test-template"))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingTestException_ReturnsExpected()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        SetupFakes(processId, _fixture.Create<MailingInformation>());
        var error = _fixture.Create<TestException>();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IReadOnlyDictionary<string, string>>._, A<string>._))
            .Throws(error);

        // Act execute
        var executionResult = await _executor.ExecuteProcessStep(ProcessStepTypeId.SEND_MAIL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert execute
        executionResult.Modified.Should().BeTrue();
        executionResult.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        executionResult.ScheduleStepTypeIds.Should().ContainInOrder(ProcessStepTypeId.RETRIGGER_SEND_MAIL);
        executionResult.SkipStepTypeIds.Should().BeNull();
        executionResult.ProcessMessage.Should().Be(error.Message);
    }

    [Fact]
    public async Task ExecuteProcessStep_ThrowingSystemException_Throws()
    {
        // Arrange initialize
        var processId = Guid.NewGuid();

        // Act initialize
        var initializationResult = await _executor.InitializeProcess(processId, _fixture.CreateMany<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert initialize
        initializationResult.Should().NotBeNull();
        initializationResult.Modified.Should().BeFalse();
        initializationResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange execute
        SetupFakes(processId, _fixture.Create<MailingInformation>());
        var error = new SystemException(_fixture.Create<string>());
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IReadOnlyDictionary<string, string>>._, A<string>._))
            .Throws(error);

        // Act execute
        async Task Act() => await _executor.ExecuteProcessStep(ProcessStepTypeId.SEND_MAIL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);
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
        result.Should().Be(ProcessTypeId.MAILING);
    }

    #endregion

    #region IsExecutableStepTypeId

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsExecutableProcessStep_ReturnsExpected(bool checklistHandlerReturnValue)
    {
        // Arrange
        var processStepTypeId = checklistHandlerReturnValue ? ProcessStepTypeId.SEND_MAIL : ProcessStepTypeId.START_AUTOSETUP;

        // Act
        var result = _executor.IsExecutableStepTypeId(processStepTypeId);

        // Assert
        result.Should().Be(checklistHandlerReturnValue);
    }

    #endregion

    #region IsLockRequested

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _executor.IsLockRequested(ProcessStepTypeId.SEND_MAIL).ConfigureAwait(false);

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

    #region Setup

    private void SetupFakes(Guid id, params MailingInformation[] mailingInformation)
    {
        A.CallTo(() => _mailingInformationRepository.GetMailingInformationForProcess(id))
            .Returns(mailingInformation.Select(x => (x.Id, x.Email, x.Template, x.MailParameter)).ToAsyncEnumerable());

        A.CallTo(() => _mailingInformationRepository.AttachAndModifyMailingInformation(A<Guid>.That.Matches(x => mailingInformation.Any(y => y.Id == x)), A<Action<MailingInformation>>._, A<Action<MailingInformation>>._))
            .Invokes((Guid id, Action<MailingInformation> initialize, Action<MailingInformation> setOptionalFields) =>
            {
                var mailing = mailingInformation.Single(x => x.Id == id);
                initialize.Invoke(mailing);
                setOptionalFields.Invoke(mailing);
            });
    }

    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor.Tests;

public class ChecklistHandlerServiceTests
{
	private readonly IBpdmBusinessLogic _bpdmBusinessLogic;
	private readonly ICustodianBusinessLogic _custodianBusinessLogic;
	private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
	private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
	private readonly IApplicationActivationService _applicationActivationService;
	private readonly IApplicationChecklistService _checklistService;
	private readonly IFixture _fixture;

	public ChecklistHandlerServiceTests()
	{
		_fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
		_fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
			.ForEach(b => _fixture.Behaviors.Remove(b));
		_fixture.Behaviors.Add(new OmitOnRecursionBehavior());

		_bpdmBusinessLogic = A.Fake<IBpdmBusinessLogic>();
		_custodianBusinessLogic = A.Fake<ICustodianBusinessLogic>();
		_clearinghouseBusinessLogic = A.Fake<IClearinghouseBusinessLogic>();
		_sdFactoryBusinessLogic = A.Fake<ISdFactoryBusinessLogic>();
		_applicationActivationService = A.Fake<IApplicationActivationService>();
		_checklistService = A.Fake<IApplicationChecklistService>();
	}

	[Theory]
	[InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER)]
	[InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER)]
	[InlineData(ProcessStepTypeId.CREATE_IDENTITY_WALLET, ApplicationChecklistEntryTypeId.IDENTITY_WALLET)]
	[InlineData(ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryTypeId.CLEARING_HOUSE)]
	[InlineData(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryTypeId.CLEARING_HOUSE)]
	[InlineData(ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP)]
	[InlineData(ProcessStepTypeId.ACTIVATE_APPLICATION, ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION)]
	public void GetProcessStepExecution_ExecutableStep_Success(ProcessStepTypeId stepTypeId, ApplicationChecklistEntryTypeId entryTypeId)
	{
		// Arrange
		var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
			Guid.NewGuid(),
			stepTypeId,
			_fixture.Create<IDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
			_fixture.CreateMany<ProcessStepTypeId>());

		var error = new TestException();

		var sut = CreateSut();

		// Act IsExecutableProcessStep
		var isExecutable = sut.IsExecutableProcessStep(stepTypeId);

		// Assert IsExecutableProcessStep
		isExecutable.Should().BeTrue();

		// Act GetProcessStepExecution
		var execution = sut.GetProcessStepExecution(stepTypeId);

		// Assert GetProcessStepExecution
		execution.EntryTypeId.Should().Be(entryTypeId);

		// Act execute process-func
		execution.ProcessFunc(context, CancellationToken.None);

		// Assert process-func
		switch (stepTypeId)
		{
			case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH:
				A.CallTo(() => _bpdmBusinessLogic.PushLegalEntity(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL:
				A.CallTo(() => _bpdmBusinessLogic.HandlePullLegalEntity(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.CREATE_IDENTITY_WALLET:
				A.CallTo(() => _custodianBusinessLogic.CreateIdentityWalletAsync(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_CLEARING_HOUSE:
				A.CallTo(() => _clearinghouseBusinessLogic.HandleClearinghouse(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE:
				A.CallTo(() => _clearinghouseBusinessLogic.HandleClearinghouse(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_SELF_DESCRIPTION_LP:
				A.CallTo(() => _sdFactoryBusinessLogic.StartSelfDescriptionRegistration(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.ACTIVATE_APPLICATION:
				A.CallTo(() => _applicationActivationService.HandleApplicationActivation(context, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
				break;
			default:
				true.Should().BeFalse($"unexpected ProcessStepTypeId: {stepTypeId}");
				break;
		}

		// Act execute error-func
		execution.ErrorFunc?.Invoke(error, context, CancellationToken.None);

		// Assert error-func
		switch (stepTypeId)
		{
			case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.CREATE_IDENTITY_WALLET:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_CLEARING_HOUSE:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.START_SELF_DESCRIPTION_LP:
				execution.ErrorFunc.Should().NotBeNull();
				A.CallTo(() => _checklistService.HandleServiceErrorAsync(error, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)).MustHaveHappenedOnceExactly();
				break;
			case ProcessStepTypeId.ACTIVATE_APPLICATION:
				execution.ErrorFunc.Should().BeNull();
				break;
			default:
				true.Should().BeFalse($"unexpected ProcessStepTypeId: {stepTypeId}");
				break;
		}
	}

	[Theory]
	[InlineData(ProcessStepTypeId.VERIFY_REGISTRATION)]
	[InlineData(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL)]
	[InlineData(ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)]
	[InlineData(ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)]
	[InlineData(ProcessStepTypeId.END_CLEARING_HOUSE)]
	[InlineData(ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)]
	[InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
	[InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
	[InlineData(ProcessStepTypeId.OVERRIDE_BUSINESS_PARTNER_NUMBER)]
	[InlineData(ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE)]
	[InlineData(ProcessStepTypeId.FINISH_SELF_DESCRIPTION_LP)]
	public void GetProcessStepExecution_InvalidStep_Throws(ProcessStepTypeId stepTypeId)
	{
		// Arrange
		var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(
			Guid.NewGuid(),
			stepTypeId,
			_fixture.Create<IDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>>().ToImmutableDictionary(),
			_fixture.CreateMany<ProcessStepTypeId>());

		var sut = CreateSut();

		var Act = () => sut.GetProcessStepExecution(stepTypeId);

		// Act
		var isExecutable = sut.IsExecutableProcessStep(stepTypeId);
		var result = Assert.Throws<ConflictException>(Act);

		// Assert
		isExecutable.Should().BeFalse();
		result.Message.Should().Be($"no execution defined for processStep {stepTypeId}");
	}

	private IApplicationChecklistHandlerService CreateSut() =>
		new ApplicationChecklistHandlerService(
			_bpdmBusinessLogic,
			_custodianBusinessLogic,
			_clearinghouseBusinessLogic,
			_sdFactoryBusinessLogic,
			_applicationActivationService,
			_checklistService);

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

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

using Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.Tests;

public class BpnDidResolverBusinessLogicTests
{
    #region Initialization

    private const string BPN = "BPNL0000000000XX";
    private static readonly Guid ApplicationId = Guid.NewGuid();

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IBpnDidResolverService _bpnDidResolverService;
    private readonly IBpnDidResolverBusinessLogic _logic;

    public BpnDidResolverBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        var portalRepository = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _bpnDidResolverService = A.Fake<IBpnDidResolverService>();
        A.CallTo(() => portalRepository.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        _logic = new BpnDidResolverBusinessLogic(portalRepository, _bpnDidResolverService);
    }

    #endregion

    #region TransmitDidAndBpn

    [Fact]
    public async Task TransmitDidAndBpn_WithBpnProcessInTodo_ProcessFails()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithRegistrationProcessInTodo_FailsProcess()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                {ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO},
                {ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE},
                {ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE},
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithRegistrationProcessInFailed_FailsProcess()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.FAILED },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithBpnProcessInFailed_FailsProcess()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.FAILED },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());

        // Act
        var result = await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Assert
        result.StepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithNotExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidAndBpnForApplicationId(ApplicationId))
            .Returns(new ValueTuple<bool, string?, string?>());
        async Task Act() => await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} does not exist");
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithEmptyDid_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidAndBpnForApplicationId(ApplicationId))
            .Returns(new ValueTuple<bool, string?, string?>(true, null, null));
        async Task Act() => await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There must be a did set");
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithEmptyBpn_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        A.CallTo(() => _applicationRepository.GetDidAndBpnForApplicationId(ApplicationId))
            .Returns(new ValueTuple<bool, string?, string?>(true, "did:web:test:1234", null));
        async Task Act() => await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There must be a bpn set");
    }

    [Fact]
    public async Task TransmitDidAndBpn_WithValid_CallsExpected()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.IN_PROGRESS },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(ApplicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        var did = "did:web:test:1234";
        A.CallTo(() => _applicationRepository.GetDidAndBpnForApplicationId(ApplicationId))
            .Returns(new ValueTuple<bool, string?, string?>(true, did, BPN));

        // Act
        var result = await _logic.TransmitDidAndBpn(context, CancellationToken.None);

        // Assert
        A.CallTo(() => _bpnDidResolverService.TransmitDidAndBpn(did, BPN, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.StepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ScheduleStepTypeIds.Should().ContainSingle(x => x == ProcessStepTypeId.REQUEST_BPN_CREDENTIAL);
    }

    #endregion
}

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

using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Tests;

public class CustodianBusinessLogicTests
{
    #region Initialization

    private static readonly Guid IdWithoutBpn = new("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid IdWithBpn = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid CompanyId = new("95c4339e-e087-4cd2-a5b8-44d385e64630");
    private const string ValidBpn = "BPNL123698762345";
    private const string ValidCompanyName = "valid company";

    private readonly IFixture _fixture;
    private readonly IApplicationRepository _applicationRepository;

    private readonly ICustodianService _custodianService;
    private readonly CustodianBusinessLogic _logic;

    public CustodianBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepository = A.Fake<IPortalRepositories>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _custodianService = A.Fake<ICustodianService>();

        A.CallTo(() => portalRepository.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        _logic = new CustodianBusinessLogic(portalRepository, _custodianService);
    }

    #endregion

    #region GetWalletByBpnAsync

    [Fact]
    public async Task GetWalletByBpnAsync_WithoutBpn_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(applicationId)).ReturnsLazily(() => (string?)null);

        // Act
        async Task Act() => await _logic.GetWalletByBpnAsync(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("BusinessPartnerNumber is not set");
    }

    [Fact]
    public async Task GetWalletByBpnAsync_WithValidData_CallsExpected()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(applicationId)).ReturnsLazily(() => ValidBpn);

        // Act
        await _logic.GetWalletByBpnAsync(applicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _custodianService.GetWalletByBpnAsync(ValidBpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Create Wallet

    [Fact]
    public async Task CreateWallet_WithBpnInProgress_Returns()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS },
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        var result = await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.ModifyChecklistEntry.Should().BeNull();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
        result.Modified.Should().BeFalse();
        result.StepStatusId.Should().Be(ProcessStepStatusId.TODO);
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateWallet_WithBpnChecklistFailed_Returns()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.FAILED }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        var result = await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.StepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateWallet_WithRegistrationVerificationFailed_Returns()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.FAILED }
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        var result = await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Modified.Should().BeTrue();
        result.StepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        A.CallTo(() => _custodianService.CreateWalletAsync(A<string>._, A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateWallet_WithNotExistingApplication_ThrowsConflictException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(applicationId, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        async Task Act() => await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {applicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task CreateWallet_WithApplicationWithoutBpn_ThrowsConflictException()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithoutBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        async Task Act() => await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {IdWithoutBpn} company {CompanyId} is empty");
    }

    [Fact]
    public async Task CreateWallet_WithValidData_CallsService()
    {
        // Arrange
        var checklist = new Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>
            {
                { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE },
                { ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO },
            }
            .ToImmutableDictionary();
        var context = new IApplicationChecklistService.WorkerChecklistProcessStepData(IdWithBpn, default, checklist, Enumerable.Empty<ProcessStepTypeId>());
        SetupForCreateWallet();

        // Act
        var result = await _logic.CreateIdentityWalletAsync(context, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _custodianService.CreateWalletAsync(ValidBpn, ValidCompanyName, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        // Assert
        result.Should().NotBe(default);

        var (stepStatusId, action, stepTypeIds, stepsToSkip, modified, processMessage) = result;

        stepStatusId.Should().Be(ProcessStepStatusId.DONE);

        stepsToSkip.Should().BeNull();

        modified.Should().BeTrue();

        stepTypeIds.Should().NotBeNull();
        stepTypeIds.Should().HaveCount(1);
        stepTypeIds!.Single().Should().Be(ProcessStepTypeId.START_CLEARING_HOUSE);

        action.Should().NotBeNull();
        var checklistEntry = new ApplicationChecklistEntry(Guid.Empty, default, default, default);
        action!(checklistEntry);

        checklistEntry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.DONE);
        checklistEntry.Comment.Should().Be("It worked.");

        processMessage.Should().BeNull();
    }

    #endregion

    #region Setup

    private void SetupForCreateWallet()
    {
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithoutBpn))
            .Returns((CompanyId, ValidCompanyName, null));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(IdWithBpn))
            .Returns((CompanyId, ValidCompanyName, ValidBpn));
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsForCreateWalletAsync(A<Guid>.That.Not.Matches(x => x == IdWithBpn || x == IdWithoutBpn)))
            .Returns(((Guid, string, string?))default);

        A.CallTo(() => _custodianService.CreateWalletAsync(ValidBpn, ValidCompanyName, CancellationToken.None))
            .ReturnsLazily(() => "It worked.");
    }

    #endregion
}

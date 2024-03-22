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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library.Tests;

public class ChecklistCreationServiceTests
{
    private static readonly Guid ApplicationWithoutBpnId = new("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid ApplicationWithBpnId = new("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid ApplicationWithChecklist = new("e100db0b-9ccd-4020-971c-7e05c0ef5780");
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly IApplicationChecklistCreationService _service;

    public ChecklistCreationServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();

        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();

        _service = new ApplicationChecklistCreationService(_portalRepositories);
    }

    #region CreateInitialChecklistAsync

    [Fact]
    public async Task CreateInitialChecklistAsync_WithBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();

        // Act
        var result = await _service.CreateInitialChecklistAsync(ApplicationWithBpnId);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithBpnId,
            A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x =>
                    x.Count() == Enum.GetValues<ApplicationChecklistEntryTypeId>().Length &&
                    x.Count(y => y.TypeId == ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ApplicationChecklistEntryStatusId.DONE) == 1 &&
                    x.Count(y => y.TypeId != ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO) == 5)))
            .MustHaveHappenedOnceExactly();

        result.Should().HaveCount(Enum.GetValues<ApplicationChecklistEntryTypeId>().Length)
            .And.Satisfy(
                x => x.TypeId == ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && x.StatusId == ApplicationChecklistEntryStatusId.DONE,
                x => x.TypeId == ApplicationChecklistEntryTypeId.IDENTITY_WALLET && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.CLEARING_HOUSE && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO
            );
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();

        // Act
        var result = await _service.CreateInitialChecklistAsync(ApplicationWithoutBpnId);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithoutBpnId,
            A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x =>
                    x.Count() == Enum.GetValues<ApplicationChecklistEntryTypeId>().Length &&
                    x.All(y => y.StatusId == ApplicationChecklistEntryStatusId.TO_DO))))
            .MustHaveHappenedOnceExactly();

        result.Should().HaveSameCount(Enum.GetValues<ApplicationChecklistEntryTypeId>())
            .And.Satisfy(
                x => x.TypeId == ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.IDENTITY_WALLET && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.CLEARING_HOUSE && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO,
                x => x.TypeId == ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION && x.StatusId == ApplicationChecklistEntryStatusId.TO_DO
            );
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutAlreadyExistingChecklist_DoesntCreateAgain()
    {
        // Arrange
        SetupFakesForCreate();

        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithChecklist);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithoutBpnId,
                A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region CreateMissingChecklistItems

    [Fact]
    public async Task CreateMissingChecklistItems_WithNoMissingItems_DoesntCreate()
    {
        // Arrange
        var existingItems = new[]
        {
            ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
            ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION
        };
        SetupFakesForCreate();

        // Act
        await _service.CreateMissingChecklistItems(ApplicationWithBpnId, existingItems);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithBpnId,
                A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateMissingChecklistItems_WithOneMissingItem_CreatesOneItem()
    {
        // Arrange
        var existingItems = new[]
        {
            ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP
        };
        SetupFakesForCreate();

        // Act
        await _service.CreateMissingChecklistItems(ApplicationWithBpnId, existingItems);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithBpnId,
            A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x =>
                    x.Count(y => y.TypeId == ApplicationChecklistEntryTypeId.IDENTITY_WALLET && y.StatusId == ApplicationChecklistEntryStatusId.TO_DO) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutMissingItem_NothingIsCalled()
    {
        // Arrange
        var existingItems = new[]
        {
            ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP
        };
        SetupFakesForCreate();

        // Act
        await _service.CreateMissingChecklistItems(ApplicationWithChecklist, existingItems);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithoutBpnId,
                A<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
    }

    #endregion

    #region GetInitialProcessStepTypeIds

    [Theory]
    [InlineData(new[] { ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION }, new[] { ApplicationChecklistEntryStatusId.TO_DO }, new[] {
            ProcessStepTypeId.VERIFY_REGISTRATION,
            ProcessStepTypeId.DECLINE_APPLICATION,
        })]
    [InlineData(new[] { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER }, new[] { ApplicationChecklistEntryStatusId.TO_DO }, new[] {
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH,
            ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL,
        })]
    [InlineData(new[] { ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER }, new[] { ApplicationChecklistEntryStatusId.IN_PROGRESS }, new ProcessStepTypeId[] { })]
    [InlineData(new[] {
            ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
        }, new[] { ApplicationChecklistEntryStatusId.TO_DO }, new ProcessStepTypeId[] { })]
    public void GetInitialProcessStepsTypeIds_ReturnsExcpected(IEnumerable<ApplicationChecklistEntryTypeId> entryTypes, IEnumerable<ApplicationChecklistEntryStatusId> entryStatus, IEnumerable<ProcessStepTypeId> stepTypeIds)
    {
        // Arrange
        var entries = entryTypes.Zip(entryStatus, (type, status) => (type, status));
        // Act
        var result = _service.GetInitialProcessStepTypeIds(entries);
        // Assert
        if (stepTypeIds.Any())
        {
            result.Should().HaveSameCount(stepTypeIds).And.Contain(stepTypeIds);
        }
        else
        {
            result.Should().BeEmpty();
        }
    }

    #endregion

    #region Setup

    private void SetupFakesForCreate()
    {
        var checklist = new List<ApplicationChecklistEntryTypeId>
        {
            ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
            ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
            ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
            ApplicationChecklistEntryTypeId.CLEARING_HOUSE
        };
        A.CallTo(() => _applicationRepository.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithBpnId))
            .Returns(("testbpn", Enumerable.Empty<ApplicationChecklistEntryTypeId>()));
        A.CallTo(() => _applicationRepository.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithoutBpnId))
            .Returns((null, Enumerable.Empty<ApplicationChecklistEntryTypeId>()));
        A.CallTo(() => _applicationRepository.GetBpnAndChecklistCheckForApplicationIdAsync(ApplicationWithChecklist))
            .Returns(("123", checklist));

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}

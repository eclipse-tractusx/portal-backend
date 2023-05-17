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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationChecklistRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    private static readonly Guid ApplicationId = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private static readonly Guid ApplicationWithExistingChecklistId = new("4f0146c6-32aa-4bb1-b844-df7e8babdcb6");

    public ApplicationChecklistRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create CreateChecklistForApplication

    [Fact]
    public async Task CreateChecklistForApplication_CreatesExpected()
    {
        // Arrange
        var checklistEntries = new[]
        {
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO),
        };
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.CreateChecklistForApplication(ApplicationId, checklistEntries);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(5);
        changedEntries.Should().AllSatisfy(x => x.State.Should().Be(EntityState.Added));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<ApplicationChecklistEntry>();
        var entries = changedEntries.Select(x => (ApplicationChecklistEntry)x.Entity);
        entries.Should().AllSatisfy(x => x.ApplicationId.Should().Be(ApplicationId));
        entries.Where(x => x.ApplicationChecklistEntryStatusId == ApplicationChecklistEntryStatusId.TO_DO).Should().HaveCount(4);
        entries.Where(x => x.ApplicationChecklistEntryStatusId == ApplicationChecklistEntryStatusId.IN_PROGRESS).Should().ContainSingle();
    }

    #endregion

    #region AttachAndModifyApplicationChecklist

    [Fact]
    public async Task AttachAndModifyApplicationChecklist_UpdatesEntry()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyApplicationChecklist(ApplicationWithExistingChecklistId, ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            entry =>
            {
                entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
                entry.Comment = "This is just a test";
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().ContainSingle();
        changedEntries.Should().AllSatisfy(x => x.State.Should().Be(EntityState.Modified));
        changedEntries.Select(x => x.Entity).Should().AllBeOfType<ApplicationChecklistEntry>();
        var entry = (ApplicationChecklistEntry)changedEntries.First().Entity;
        entry.DateLastChanged.Should().NotBeNull();
        entry.ApplicationChecklistEntryStatusId.Should().Be(ApplicationChecklistEntryStatusId.IN_PROGRESS);
        entry.Comment.Should().Be("This is just a test");
    }

    #endregion

    #region GetChecklistData

    [Fact]
    public async Task GetChecklistData_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("1f9a3232-9772-4ecb-8f50-c16e97772dfe");
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetChecklistData(processId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull()
            .And.BeOfType<(bool IsValidProcessId, Guid ApplicationId, CompanyApplicationStatusId ApplicationStatusId, IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)> Checklist)>()
            .And.Match<(bool IsValidProcessId, Guid ApplicationId, CompanyApplicationStatusId ApplicationStatusId, IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)> Checklist)>(
                x => x.IsValidProcessId && x.ApplicationId == new Guid("6b2d1263-c073-4a48-bfaf-704dc154ca9f") && x.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED && x.Checklist.Count() == 6
            );
    }

    #endregion

    #region GetChecklistProcessStepData

    [Fact]
    public async Task GetChecklistProcessStepData_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetChecklistProcessStepData(new Guid("6b2d1263-c073-4a48-bfaf-704dc154ca9f"),
            new[]
            {
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
            },
            new[]
            {
                ProcessStepTypeId.START_CLEARING_HOUSE,
            }
            ).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result!.IsSubmitted.Should().BeTrue();
        result.Checklist.Should().HaveCount(5).And.Contain(new[] {
            ( ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE ),
            ( ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE ),
            ( ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE ),
            ( ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE ),
            ( ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE ),
        });
        result.Process!.Id.Should().Be(new Guid("1f9a3232-9772-4ecb-8f50-c16e97772dfe"));
        result.Process.ProcessTypeId.Should().Be(ProcessTypeId.APPLICATION_CHECKLIST);
        result.Process.LockExpiryDate.Should().Be(DateTimeOffset.Parse("2023-03-01 00:00:00.000000 +00:00"));
        result.Process.IsLocked().Should().BeTrue();
        result.Process.IsLockExpired(DateTimeOffset.Parse("2023-02-28 00:00:00.000000 +00:00")).Should().BeFalse();
        result.Process.IsLockExpired(DateTimeOffset.Parse("2023-03-01 00:00:00.000000 +00:00")).Should().BeFalse();
        result.Process.IsLockExpired(DateTimeOffset.Parse("2023-03-02 00:00:00.000000 +00:00")).Should().BeTrue();
        result.ProcessSteps.Should().NotBeEmpty();
        result.ProcessSteps!.Select(step => (step.ProcessStepTypeId, step.ProcessStepStatusId, step.ProcessId)).Should().Contain(new[] {
            (ProcessStepTypeId.START_CLEARING_HOUSE, ProcessStepStatusId.TODO, new Guid ("1f9a3232-9772-4ecb-8f50-c16e97772dfe")),
        });
    }

    [Fact]
    public async Task GetChecklistProcessStepData_WithNotExisting_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetChecklistProcessStepData(Guid.NewGuid(),
            new[]
            {
                ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER,
                ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                ApplicationChecklistEntryTypeId.IDENTITY_WALLET,
                ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP,
            },
            new[]
            {
                ProcessStepTypeId.START_CLEARING_HOUSE,
            }
        ).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private async Task<(ApplicationChecklistRepository, PortalDbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationChecklistRepository(context);
        return (sut, context);
    }

    private async Task<ApplicationChecklistRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationChecklistRepository(context);
        return sut;
    }
}

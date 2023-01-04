/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationChecklistRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    
    private readonly static Guid ApplicationId = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");
    private readonly static Guid ApplicationWithExistingChecklistId = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");

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
            new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(ChecklistEntryTypeId.CLEARING_HOUSE, ChecklistEntryStatusId.TO_DO),
            new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(ChecklistEntryTypeId.IDENTITY_WALLET, ChecklistEntryStatusId.TO_DO),
            new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(ChecklistEntryTypeId.SELF_DESCRIPTION_LP, ChecklistEntryStatusId.TO_DO),
            new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.IN_PROGRESS),
            new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(ChecklistEntryTypeId.REGISTRATION_VERIFICATION, ChecklistEntryStatusId.TO_DO),
        };
        var (sut, context) = await CreateSut().ConfigureAwait(false);

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
        entries.Where(x => x.StatusId == ChecklistEntryStatusId.TO_DO).Should().HaveCount(4);
        entries.Where(x => x.StatusId == ChecklistEntryStatusId.IN_PROGRESS).Should().ContainSingle();
    }

    #endregion

    #region AttachAndModifyApplicationChecklist

    
    [Fact]
    public async Task AttachAndModifyApplicationChecklist_UpdatesEntry()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyApplicationChecklist(ApplicationWithExistingChecklistId, ChecklistEntryTypeId.REGISTRATION_VERIFICATION,
            entry =>
            {
                entry.StatusId = ChecklistEntryStatusId.IN_PROGRESS;
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
        entry.StatusId.Should().Be(ChecklistEntryStatusId.IN_PROGRESS);
        entry.Comment.Should().Be("This is just a test");
    }

    #endregion
    
    private async Task<(ApplicationChecklistRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ApplicationChecklistRepository(context);
        return (sut, context);
    }
}

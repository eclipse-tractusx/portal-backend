/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Tests.Setup;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ConsentRepositoryTests"/>
/// </summary>
public class ConsentRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ConsentRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create Consent

    [Fact]
    public async Task CreateConsent_ReturnsExpectedAppCount()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.CreateConsent(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019951"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), ConsentStatusId.ACTIVE, consent =>
        {
            consent.Comment = "Only a test comment";
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.Comment.Should().Be("Only a test comment");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Added);
        changedEntity.Entity.Should().BeOfType<Consent>().Which.Comment.Should().Be("Only a test comment");
    }

    #endregion

    #region GetConsentDetailData

    [Fact]
    public async Task GetConsentDetailData_WithValidId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConsentDetailData(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"));

        // Assert
        result.Should().NotBeNull();
        result!.ConsentStatus.Should().Be(ConsentStatusId.INACTIVE);
    }

    [Fact]
    public async Task GetConsentDetailData_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConsentDetailData(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Remove Consents

    [Fact]
    public async Task RemoveConsents_WithExistingConsent_RemovesConsent()
    {
        // Arrange
        var (sut, dbContext) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.RemoveConsents(new [] { dbContext.Consents.First() });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
        changedEntity.Entity.Should().BeOfType<Consent>().Which.Comment.Should().Be("Just a test");
    }

    #endregion

    #region Remove Consents

    [Fact]
    public async Task AttachToDatabase_WithExistingConsent_AttachesTheEntity()
    {
        // Arrange
        var (sut, dbContext) = await CreateSut().ConfigureAwait(false);
        var consent = new Consent(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"));

        // Act
        sut.AttachToDatabase(new []{consent});
        consent.Comment = "Changed comment";
        
        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<Consent>().Which.Comment.Should().Be("Changed comment");
    }

    #endregion

    private async Task<(ConsentRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        _fixture.Inject(context);
        var sut = _fixture.Create<ConsentRepository>();
        return (sut, context);
    }
}

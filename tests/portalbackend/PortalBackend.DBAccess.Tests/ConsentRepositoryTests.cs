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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ConsentRepositoryTests"/>
/// </summary>
public class ConsentRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public ConsentRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
        var result = await sut.GetConsentDetailData(new Guid("925d02e7-0ef4-4a47-a087-0bdf6af4f4f5"), OfferTypeId.SERVICE);

        // Assert
        result.Should().NotBeNull();
        result!.ConsentStatus.Should().Be(ConsentStatusId.ACTIVE);
    }

    [Fact]
    public async Task GetConsentDetailData_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConsentDetailData(Guid.NewGuid(), OfferTypeId.APP);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConsentDetailData_WithInvalidOfferType_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetConsentDetailData(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"), OfferTypeId.SERVICE);

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
        changedEntity.Entity.Should().BeOfType<Consent>();
    }

    #endregion

    #region AttachAndModifiesConsents

    [Fact]
    public async Task AttachAndModifiesConsents_WithValidConsents_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifiesConsents(new []
            {
                new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"),
                new Guid("ac1cf001-7fbc-1f2f-817f-bce058019911")
            },
            consent => { consent.ConsentStatusId = ConsentStatusId.ACTIVE; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changedEntries.Should().HaveCount(2);
        changedEntries.Should().AllSatisfy(x => x.Entity.Should().BeOfType<Consent>().Which.ConsentStatusId.Should().Be(ConsentStatusId.ACTIVE));
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(2);
        changedEntries.Should().AllSatisfy(x => x.Entity.Should().BeOfType<Consent>().Which.ConsentStatusId.Should().Be(ConsentStatusId.ACTIVE));
    }

    #endregion
    
    private async Task<(ConsentRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ConsentRepository(context);
        return (sut, context);
    }
}

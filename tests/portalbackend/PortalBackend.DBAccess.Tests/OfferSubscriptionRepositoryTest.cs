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
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class OfferSubscriptionRepositoryTest : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public OfferSubscriptionRepositoryTest(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task AttachAndModifyOfferSubscription_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        var offerSubscriptionId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd");
        var modifiedName = "Modified Name";

        // Act
        sut.AttachAndModifyOfferSubscription(offerSubscriptionId,
            sub =>
            {
                sub.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
                sub.DisplayName = modifiedName;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<OfferSubscription>().Which.Should().Match<OfferSubscription>(os =>
            os.Id == offerSubscriptionId &&
            os.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING &&
            os.DisplayName == modifiedName);
    }

    #endregion

    #region GetOfferSubscriptionStateForCompany

    [Fact]
    public async Task GetOfferSubscriptionStateForCompanyAsync_WithExistingData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferSubscriptionStateForCompanyAsync(
            new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"), 
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), 
            OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(default);
        result.offerSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.offerSubscriptionId.Should().Be(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));
    }

    [Fact]
    public async Task GetOfferSubscriptionStateForCompanyAsync_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferSubscriptionStateForCompanyAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), 
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), 
            OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion
    
    #region GetAllBusinessAppDataForUserId

    [Fact]
    public async Task GetAllBusinessAppDataForUserIdAsync_WithValidUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetAllBusinessAppDataForUserIdAsync("502dabcf-01c7-47d9-a88e-0be4279097b5").ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.First().SubscriptionUrl.Should().Be("https://ec-qas.d13fe27.kyma.ondemand.com");
    }

    #endregion

    #region GetOwnCompanyProvidedOfferSubscriptionStatusesUntracked
    
    [Theory]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc)]
    [InlineData(SubscriptionStatusSorting.OfferIdDesc)]
    [InlineData(SubscriptionStatusSorting.CompanyNameAsc)]
    [InlineData(SubscriptionStatusSorting.CompanyNameDesc)]
    public async Task GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync_ReturnsExpectedNotificationDetailData(SubscriptionStatusSorting sorting)
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync("8be5ee49-4b9c-4008-b641-138305430cc4", OfferTypeId.SERVICE, sorting, OfferSubscriptionStatusId.ACTIVE)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Count.Should().Be(1);
        results.Data.Should().HaveCount(1);
        results.Data.Should().AllBeOfType<OfferCompanySubscriptionStatusData>().Which.First().CompanySubscriptionStatuses.Should().HaveCount(1);
    }
    
    #endregion
    
    #region GetOfferDetailsAndCheckUser

    [Fact]
    public async Task GetOfferDetailsAndCheckUser_WithValidUserandSubscriptionId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferDetailsAndCheckUser(new Guid("85064593-0aaa-48c4-95a2-2700223aca5e"), "502dabcf-01c7-47d9-a88e-0be4279097b5", OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(default);
        result!.OfferId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce05748000d"));
        result!.Status.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result!.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result!.CompanyName.Should().Be("Catena-X");
    }

    #endregion

    #region Setup
    
    private async Task<(OfferSubscriptionsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new OfferSubscriptionsRepository(context);
        return (sut, context);
    }
    
    #endregion
}

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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class OfferSubscriptionRepositoryTest : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _userCompanyId = new("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd");

    public OfferSubscriptionRepositoryTest(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region AttachAndModifyOfferSubscription

    [Fact]
    public async Task AttachAndModifyOfferSubscription_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut();

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
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckPendingOrActiveSubscriptionExists(
            new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"),
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            OfferTypeId.SERVICE);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOfferSubscriptionStateForCompanyAsync_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckPendingOrActiveSubscriptionExists(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"),
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            OfferTypeId.SERVICE);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAllBusinessAppDataForUserId

    [Fact]
    public async Task GetAllBusinessAppDataForUserIdAsync_WithValidUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetAllBusinessAppDataForUserIdAsync(new("ac1cf001-7fbc-1f2f-817f-bce058020006")).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.First().SubscriptionUrl.Should().Be("https://ec-qas.d13fe27.kyma.ondemand.com");
        result.First().OfferId.Should().Be(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"));
    }

    #endregion

    #region GetOwnCompanyProvidedOfferSubscriptionStatusesUntracked

    [Theory]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 2, true, 2)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.SERVICE, new OfferSubscriptionStatusId[] { }, 0, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdDesc, null, OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 2, false, 1)]
    [InlineData(SubscriptionStatusSorting.CompanyNameAsc, null, OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 2, true, 2)]
    [InlineData(SubscriptionStatusSorting.CompanyNameDesc, null, OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 2, true, 2)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, "a16e73b9-5277-4b69-9f8d-3b227495dfea", OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 1, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, "a16e73b9-5277-4b69-9f8d-3b227495dfae", OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 1, true, 2)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, "deadbeef-dead-beef-dead-beefdeadbeef", OfferTypeId.SERVICE, new[] { OfferSubscriptionStatusId.ACTIVE }, 0, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.APP, new[] { OfferSubscriptionStatusId.ACTIVE }, 1, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.APP, new[] { OfferSubscriptionStatusId.INACTIVE }, 1, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.APP, new[] { OfferSubscriptionStatusId.PENDING }, 1, false, 1)]
    [InlineData(SubscriptionStatusSorting.OfferIdAsc, null, OfferTypeId.APP, new[] { OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE }, 2, false, 1)]
    [InlineData(null, null, OfferTypeId.APP, new[] { OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE }, 2, false, 1)]
    public async Task GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync_ReturnsExpectedNotificationDetailData(SubscriptionStatusSorting? sorting, string? offerIdTxt, OfferTypeId offerTypeId, IEnumerable<OfferSubscriptionStatusId> offerSubscriptionStatusIds, int count, bool technicalUser, int companySubscriptionCount)
    {
        // Arrange
        Guid? offerId = offerIdTxt == null ? null : new Guid(offerIdTxt);
        var (sut, _) = await CreateSut();

        // Act
        var results = await sut.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_userCompanyId, offerTypeId, sorting, offerSubscriptionStatusIds, offerId, null)(0, 15);

        // Assert
        if (count > 0)
        {
            results.Should().NotBeNull();
            results!.Count.Should().Be(count);
            results.Data.Should().HaveCount(count)
                .And.AllBeOfType<OfferCompanySubscriptionStatusData>()
                .Which.First().CompanySubscriptionStatuses.Should().Match(x =>
                    x.Count() == companySubscriptionCount &&
                    x.First().TechnicalUser == technicalUser);
        }
        else
        {
            results.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync_WithCompanyFilter_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var results = await sut.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_userCompanyId, OfferTypeId.SERVICE, SubscriptionStatusSorting.CompanyNameAsc, new[] { OfferSubscriptionStatusId.ACTIVE, OfferSubscriptionStatusId.PENDING }, null, "catena")(0, 15);

        // Assert
        results.Should().NotBeNull();
        results!.Count.Should().Be(2);
        results.Data.Should().HaveCount(2);
        results.Data.Should().AllBeOfType<OfferCompanySubscriptionStatusData>().Which.First().CompanySubscriptionStatuses.Should().HaveCount(1);
    }

    #endregion

    #region GetOfferDetailsAndCheckUser

    [Fact]
    public async Task GetOfferDetailsAndCheckUser_WithValidUserAndSubscriptionId_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOfferDetailsAndCheckProviderCompany(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"), new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferTypeId.APP);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(default);
        result!.OfferId.Should().Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"));
        result.Status.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.CompanyName.Should().Be("Catena-X");
        result.IsProviderCompany.Should().BeTrue();
        result.Bpn.Should().Be("BPNL00000003CRHK");
        result.OfferName.Should().Be("Trace-X");
        result.InstanceData.IsSingleInstance.Should().BeTrue();
        result.InstanceData.InstanceUrl.Should().Be("https://test.com");
    }

    [Fact]
    public async Task GetOfferDetailsAndCheckUser_WithSubscriptionForOfferWithoutAppInstanceSetup_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOfferDetailsAndCheckProviderCompany(new Guid("e80b5f5c-3a16-480b-b82e-1cc06a71fddc"), new("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"), OfferTypeId.SERVICE);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(default);
        result!.OfferId.Should().Be(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfae"));
        result.Status.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        result.CompanyName.Should().Be("Catena-X");
        result.IsProviderCompany.Should().BeTrue();
        result.Bpn.Should().Be("BPNL00000003CRHK");
        result.OfferName.Should().Be("Service Test 123");
        result.InstanceData.IsSingleInstance.Should().BeFalse();
        result.InstanceData.InstanceUrl.Should().BeNull();
    }

    #endregion

    #region GetSubscriptionDetailForProviderAsync

    [Fact]
    public async Task GetSubscriptionDetailForProviderAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForProviderAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId, OfferTypeId.SERVICE, new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") });

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeTrue();
        result.Details.Should().NotBeNull().And.Match<ProviderSubscriptionDetailData>(x =>
            x.Name == "SDE with EDC" &&
            x.Customer == "Catena-X" &&
            x.Contact.SequenceEqual(new[] { "tobeadded@cx.com" }) &&
            x.OfferSubscriptionStatus == OfferSubscriptionStatusId.ACTIVE
            && x.TechnicalUserData.All(x => x.Id == new Guid("d0c8ae19-d4f3-49cc-9cb4-6c766d4680f2") && x.Name == "sa-x-4"));
    }

    [Fact]
    public async Task GetSubscriptionDetailForProviderAsync_WithNotExistingId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForProviderAsync(Guid.NewGuid(), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId, OfferTypeId.SERVICE, new List<Guid>());

        // Assert
        result.Exists.Should().BeFalse();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    [Fact]
    public async Task GetSubscriptionDetailForProviderAsync_WithWrongUser_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForProviderAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), Guid.NewGuid(), OfferTypeId.SERVICE, new List<Guid>());

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    #endregion

    #region GetAppSubscriptionDetailForProviderAsync

    [Fact]
    public async Task GetAppSubscriptionDetailForProviderAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetAppSubscriptionDetailsForProviderAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId, OfferTypeId.SERVICE, new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") });

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeTrue();
        result.Details.Should().NotBeNull().And.Match<AppProviderSubscriptionDetail>(x =>
            x.Name == "SDE with EDC" &&
            x.Customer == "Catena-X" &&
            x.Contact.SequenceEqual(new[] { "tobeadded@cx.com" }) &&
            x.OfferSubscriptionStatus == OfferSubscriptionStatusId.ACTIVE &&
            x.TenantUrl == "https://ec-qas.d13fe27.kyma.ondemand.com" &&
            x.AppInstanceId == "https://catenax-int-dismantler-s66pftcc.authentication.eu10.hana.ondemand.com" &&
            x.ProcessSteps.Count() == 3 &&
            x.ProcessSteps.Count(y => y.ProcessStepTypeId == ProcessStepTypeId.START_AUTOSETUP && y.ProcessStepStatusId == ProcessStepStatusId.TODO) == 1);
    }

    [Fact]
    public async Task GetAppSubscriptionDetailForProviderAsync_WithNotExistingId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetAppSubscriptionDetailsForProviderAsync(Guid.NewGuid(), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId, OfferTypeId.SERVICE, new List<Guid>());

        // Assert
        result.Exists.Should().BeFalse();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    [Fact]
    public async Task GetAppSubscriptionDetailForProviderAsync_WithWrongUserCompany_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetAppSubscriptionDetailsForProviderAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), Guid.NewGuid(), OfferTypeId.SERVICE, new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") });

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    #endregion

    #region GetSubscriptionDetailForSubscriberAsync

    [Fact]
    public async Task GetSubscriptionDetailForSubscriberAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForSubscriberAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferTypeId.SERVICE, new[] { new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632") });

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeTrue();
        result.Details.Should().NotBeNull().And.Match<SubscriberSubscriptionDetailData>(x =>
            x.Name == "SDE with EDC" &&
            x.Provider == "Service Provider" &&
            x.Contact.SequenceEqual(new string[] { "service.provider@acme.corp" }) &&
            x.OfferSubscriptionStatus == OfferSubscriptionStatusId.ACTIVE &&
            x.ConnectorData.SequenceEqual(new[]{ new SubscriptionAssignedConnectorData(
                new Guid("bd644d9c-ca12-4488-ae38-6eb902c9bec0"),
                "Test Connector 123",
                "www.google.de")}));
    }

    [Fact]
    public async Task GetSubscriptionDetailForSubscriberAsync_WithNotExistingId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForSubscriberAsync(Guid.NewGuid(), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId, OfferTypeId.SERVICE, new List<Guid>());

        // Assert
        result.Exists.Should().BeFalse();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    [Fact]
    public async Task GetSubscriptionDetailForSubscriberAsync_WithWrongUser_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionDetailsForSubscriberAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), Guid.NewGuid(), OfferTypeId.SERVICE, new List<Guid>());

        // Assert
        result.Exists.Should().BeTrue();
        result.IsUserOfCompany.Should().BeFalse();
        result.Details.Should().BeNull();
    }

    #endregion

    #region GetOfferSubscriptionDataForProcessIdAsync

    [Fact]
    public async Task GetOfferSubscriptionDataForProcessIdAsync_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOfferSubscriptionDataForProcessIdAsync(new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe06"));

        // Assert
        result.Should().NotBe(Guid.Empty);
        result.Should().Be(new Guid("e8886159-9258-44a5-88d8-f5735a197a09"));
    }

    [Fact]
    public async Task GetOfferSubscriptionDataForProcessIdAsync_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOfferSubscriptionDataForProcessIdAsync(Guid.NewGuid());

        // Assert
        result.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetOfferSubscriptionDataForProcessIdAsync

    [Fact]
    public async Task GetTriggerProviderInformation_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTriggerProviderInformation(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result!.OfferName.Should().Be("Trace-X");
        result.IsSingleInstance.Should().BeTrue();
    }

    [Fact]
    public async Task GetTriggerProviderInformation_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTriggerProviderInformation(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSubscriptionActivationDataByIdAsync

    [Fact]
    public async Task GetSubscriptionActivationDataByIdAsync_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionActivationDataByIdAsync(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result!.OfferName.Should().Be("Trace-X");
        result.InstanceData.Should().Be((true, "https://test.com"));
        result.Status.Should().Be(OfferSubscriptionStatusId.ACTIVE);
    }

    [Fact]
    public async Task GetSubscriptionActivationDataByIdAsync_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetSubscriptionActivationDataByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetProcessStepData

    [Fact]
    public async Task GetProcessStepData_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetProcessStepData(new Guid("e8886159-9258-44a5-88d8-f5735a197a09"), new[]
        {
            ProcessStepTypeId.START_AUTOSETUP
        });

        // Assert
        result.Should().NotBeNull();
        result!.ProcessSteps.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProcessStepData_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetProcessStepData(Guid.NewGuid(), new[] { ProcessStepTypeId.START_AUTOSETUP });

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsActiveOfferSubscription

    [Fact]
    public async Task IsActiveOfferSubscription_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.IsActiveOfferSubscription(new Guid("e8886159-9258-44a5-88d8-f5735a197a09"));

        // Assert
        result.Should().NotBeNull();
        result!.IsValidSubscriptionId.Should().BeTrue();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveOfferSubscription_WithActive_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.IsActiveOfferSubscription(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result!.IsValidSubscriptionId.Should().BeTrue();
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveOfferSubscription_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.IsActiveOfferSubscription(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.IsValidSubscriptionId.Should().BeFalse();
    }

    #endregion

    #region GetClientCreationData

    [Fact]
    public async Task GetClientCreationData_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetClientCreationData(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result!.OfferType.Should().Be(OfferTypeId.APP);
        result.IsTechnicalUserNeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetClientCreationData_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetClientCreationData(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTechnicalUserCreationData

    [Fact]
    public async Task GetTechnicalUserCreationData_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserCreationData(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result!.Bpn.Should().Be("BPNL00000003CRHK");
        result.OfferName.Should().Be("Trace-X");
        result.IsTechnicalUserNeeded.Should().BeTrue();
    }

    [Fact]
    public async Task GetTechnicalUserCreationData_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserCreationData(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTriggerProviderCallbackInformation

    [Fact]
    public async Task GetTriggerProviderCallbackInformation_WithValidData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTriggerProviderCallbackInformation(new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OfferSubscriptionStatusId.ACTIVE);
    }

    [Fact]
    public async Task GetTriggerProviderCallbackInformation_WithNotExistingId_ReturnsNull()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetTriggerProviderCallbackInformation(Guid.NewGuid());

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region Create Notification

    [Fact]
    public async Task CreateNotification_ReturnsExpectedResult()
    {
        // Arrange
        var offerSubscriptionId = new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba");
        var (sut, context) = await CreateSut();

        // Act
        var results = sut.CreateOfferSubscriptionProcessData(offerSubscriptionId, "https://www.test.de");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.OfferUrl.Should().Be("https://www.test.de");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Should().Match<EntityEntry>(x =>
                x.State == EntityState.Added &&
                x.Entity.GetType() == typeof(OfferSubscriptionProcessData) &&
                ((OfferSubscriptionProcessData)x.Entity).OfferSubscriptionId == offerSubscriptionId &&
                ((OfferSubscriptionProcessData)x.Entity).OfferUrl == "https://www.test.de");
    }

    #endregion

    #region RemoveOfferSubscriptionProcessData

    [Fact]
    public async Task RemoveOfferSubscriptionProcessData_WithExisting_RemovesOfferSubscriptionProcessData()
    {
        // Arrange
        var id = Guid.NewGuid();
        var (sut, dbContext) = await CreateSut();

        // Act
        sut.RemoveOfferSubscriptionProcessData(id);

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Should().Match<EntityEntry>(x =>
                x.State == EntityState.Deleted &&
                x.Entity.GetType() == typeof(OfferSubscriptionProcessData) &&
                ((OfferSubscriptionProcessData)x.Entity).Id == id);
    }

    #endregion

    #region GetUpdateUrlDataAsync

    [Fact]
    public async Task GetUpdateUrlDataAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetUpdateUrlDataAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId);

        // Assert
        result.Should().NotBeNull();
        result!.IsUserOfCompany.Should().BeTrue();
    }

    [Fact]
    public async Task GetUpdateUrlDataAsync_WithNotExistingId_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetUpdateUrlDataAsync(Guid.NewGuid(), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), _userCompanyId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUpdateUrlDataAsync_WithWrongUser_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetUpdateUrlDataAsync(new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea"), new Guid("3DE6A31F-A5D1-4F60-AA3A-4B1A769BECBF"), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result!.IsUserOfCompany.Should().BeFalse();
        result.OfferName.Should().Be("SDE with EDC");
    }

    #endregion

    #region AttachAndModifyAppSubscriptionDetail

    [Theory]
    [InlineData("https://www.new-url.com")]
    [InlineData(null)]
    public async Task AttachAndModifyAppSubscriptionDetail_ReturnsExpectedResult(string? modifiedUrl)
    {
        // Arrange
        var (sut, context) = await CreateSut();

        var detailId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93ca");
        var offerSubscriptionId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd");

        // Act
        sut.AttachAndModifyAppSubscriptionDetail(detailId, offerSubscriptionId,
            os =>
            {
                os.AppSubscriptionUrl = "https://test.com";
            },
            sub =>
            {
                sub.AppSubscriptionUrl = modifiedUrl;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<AppSubscriptionDetail>()
            .Which.AppSubscriptionUrl.Should().Be(modifiedUrl);
    }

    [Theory]
    [InlineData("https://www.new-url.com")]
    [InlineData(null)]
    public async Task AttachAndModifyAppSubscriptionDetail__WithUnchangedUrl_DoesntUpdate(string? modifiedUrl)
    {
        // Arrange
        var (sut, context) = await CreateSut();

        var detailId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93ca");
        var offerSubscriptionId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd");

        // Act
        sut.AttachAndModifyAppSubscriptionDetail(detailId, offerSubscriptionId,
            os =>
            {
                os.AppSubscriptionUrl = modifiedUrl;
            },
            sub =>
            {
                sub.AppSubscriptionUrl = modifiedUrl;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeFalse();
    }

    #endregion

    #region  GetOwnCompanySubscribedOfferSubscriptionStatuse

    [Theory]
    [InlineData("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87", OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE)]
    [InlineData("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87", OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE)]
    [InlineData("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87", OfferTypeId.CORE_COMPONENT, DocumentTypeId.SERVICE_LEADIMAGE)]
    public async Task GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync_ReturnsExpected(Guid companyId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(companyId, offerTypeId, documentTypeId)(0, 15);

        // Assert
        switch (offerTypeId)
        {
            case OfferTypeId.APP:
                result.Should().NotBeNull();
                result!.Data.Should().HaveCount(2).And.Satisfy(
                    x => x.OfferId == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007") &&
                        x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                        x.OfferName == "Trace-X" &&
                        x.Provider == "Catena-X" &&
                        x.OfferSubscriptionId == new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba") &&
                        x.DocumentId == new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b1"),
                    x => x.OfferId == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007") &&
                        x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING &&
                        x.OfferName == "Trace-X" &&
                        x.Provider == "Catena-X" &&
                        x.OfferSubscriptionId == new Guid("e8886159-9258-44a5-88d8-f5735a197a09") &&
                        x.DocumentId == new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b1")
                );
                break;

            case OfferTypeId.SERVICE:
                result.Should().NotBeNull();
                result!.Data.Should().HaveCount(2).And.Satisfy(
                    x => x.OfferId == new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfea") &&
                        x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                        x.OfferName == "SDE with EDC" &&
                        x.Provider == "Service Provider" &&
                        x.OfferSubscriptionId == new Guid("3de6a31f-a5d1-4f60-aa3a-4b1a769becbf") &&
                        x.DocumentId == Guid.Empty,
                    x => x.OfferId == new Guid("a16e73b9-5277-4b69-9f8d-3b227495dfae") &&
                        x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE &&
                        x.OfferName == "Service Test 123" &&
                        x.Provider == "Service Provider" &&
                        x.OfferSubscriptionId == new Guid("e80b5f5c-3a16-480b-b82e-1cc06a71fddc") &&
                        x.DocumentId == Guid.Empty
                );
                break;

            case OfferTypeId.CORE_COMPONENT:
                result.Should().BeNull();
                break;
        }
    }

    #endregion

    #region GetProcessStepsForSubscription

    [Fact]
    public async Task GetProcessStepsForSubscription_WithExisting_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetProcessStepsForSubscription(new Guid("e8886159-9258-44a5-88d8-f5735a197a09")).ToListAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProcessStepsForSubscription_WithoutExisting_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetProcessStepsForSubscription(Guid.NewGuid()).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CheckOfferSubscriptionWithOfferProvider

    [Fact]
    public async Task CheckOfferSubscriptionWithOfferProvider_WithExisting_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckOfferSubscriptionWithOfferProvider(new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"), new Guid("0dcd8209-85e2-4073-b130-ac094fb47106"));

        // Assert
        result.Exists.Should().BeTrue();
        result.OfferSubscriptionStatus.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.ProviderBpn.Should().Be("BPNL00000003AYRE");
        result.IsOfferProvider.Should().BeTrue();
    }

    [Fact]
    public async Task CheckOfferSubscriptionWithOfferProvider_WithExistingAndNotOfferProvider_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckOfferSubscriptionWithOfferProvider(new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"), Guid.NewGuid());

        // Assert
        result.Exists.Should().BeTrue();
        result.OfferSubscriptionStatus.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.ProviderBpn.Should().Be("BPNL00000003AYRE");
        result.IsOfferProvider.Should().BeFalse();
    }

    [Fact]
    public async Task CheckOfferSubscriptionWithOfferProvider_WithoutExisting_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckOfferSubscriptionWithOfferProvider(Guid.NewGuid(), new Guid("0dcd8209-85e2-4073-b130-ac094fb47106"));

        // Assert
        result.Exists.Should().BeFalse();
    }

    #endregion

    #region GetConnectorOfferSubscriptionData

    [Fact]
    public async Task GetConnectorOfferSubscriptionData_WithoutFilter_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorOfferSubscriptionData(null, new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999")).ToListAsync();

        // Assert
        result.Should().HaveCount(3)
            .And.Satisfy(
                x => x.SubscriptionId == new Guid("014afd09-e51a-4ecf-83ab-a5380d9af832"),
                x => x.SubscriptionId == new Guid("92be9d79-4064-422c-bdc8-a12ca7d26e5d"),
                x => x.SubscriptionId == new Guid("ed6065b1-0902-4d5e-9470-33a716022a1a"));
    }

    [Fact]
    public async Task GetConnectorOfferSubscriptionData_WithConnectorIdSet_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorOfferSubscriptionData(true, new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999")).ToListAsync();

        // Assert
        result.Should().HaveCount(2).And.AllSatisfy(x => x.ConnectorIds.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task GetConnectorOfferSubscriptionData_WithoutConnectorIdSet_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetConnectorOfferSubscriptionData(false, new Guid("41fd2ab8-7123-4546-9bef-a388d91b2999")).ToListAsync();

        // Assert
        result.Should().HaveCount(1).And.AllSatisfy(x => x.ConnectorIds.Should().BeEmpty());
    }

    #endregion

    #region GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync

    [Fact]
    public async Task GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE).ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1).And.Satisfy(
            x => x.OfferId == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007") &&
                x.OfferName == "Trace-X" &&
                x.Provider == "Catena-X" &&
                x.DocumentId == new Guid("e020787d-1e04-4c0b-9c06-bd1cd44724b1") &&
                x.OfferSubscriptionId == new Guid("ed4de48d-fd4b-4384-a72f-ecae3c6cc5ba"));
    }

    [Fact]
    public async Task GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanyActiveSubscribedOfferSubscriptionStatusesUntrackedAsync(Guid.NewGuid(), OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCompanyActiveSubscribedOfferSubscriptionStatuses

    [Fact]
    public async Task GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferTypeId.APP).ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2).And.Satisfy(
            x => x.OfferId == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007") &&
                x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE,
            x => x.OfferId == new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007") &&
                x.OfferSubscriptionStatusId == OfferSubscriptionStatusId.PENDING);
    }

    [Fact]
    public async Task GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetOwnCompanySubscribedOfferSubscriptionUntrackedAsync(Guid.NewGuid(), OfferTypeId.APP).ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CheckOfferSubscriptionForProvider

    [Fact]
    public async Task CheckOfferSubscriptionForProvider_WithProvidingCompany_ReturnsTrue()
    {

        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckOfferSubscriptionForProvider(new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"), new Guid("0dcd8209-85e2-4073-b130-ac094fb47106"));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckOfferSubscriptionForProvider_WithoutProvidingCompany_ReturnsTrue()
    {

        // Arrange
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.CheckOfferSubscriptionForProvider(new Guid("0b2ca541-206d-48ad-bc02-fb61fbcb5552"), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion
    #region Create OfferSubscription

    [Fact]
    public async Task CreateOfferSubscription_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        // Act
        var results = sut.CreateOfferSubscription(new Guid("ac1cf001-7fbc-1f2f-817f-bce0572c0007"),
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferSubscriptionStatusId.PENDING, default);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().State.Should().Be(EntityState.Added);
        changedEntries.Single().Entity.Should().BeOfType<OfferSubscription>().Which.CompanyId.Should().Be(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));
    }

    #endregion

    #region Setup

    private async Task<(IOfferSubscriptionsRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new OfferSubscriptionsRepository(context);
        return (sut, context);
    }

    #endregion
}

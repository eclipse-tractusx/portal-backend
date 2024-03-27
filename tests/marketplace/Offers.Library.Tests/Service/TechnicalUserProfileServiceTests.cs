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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class TechnicalUserProfileServiceTests
{
    private const string OfferName = "Super App";
    private readonly Guid _offerId = Guid.NewGuid();
    private readonly Guid _offerSubscriptionId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly TechnicalUserProfileService _sut;

    public TechnicalUserProfileServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        _sut = new TechnicalUserProfileService(_portalRepositories);
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithoutOffer_ThrowsException()
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId, A<OfferTypeId>._))
            .Returns<(bool, IEnumerable<IEnumerable<UserRoleData>>, string?)>(default);

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(_offerId, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer APP {_offerId} does not exists");
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithoutOfferName_ThrowsException()
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId, A<OfferTypeId>._))
            .Returns((true, Enumerable.Empty<IEnumerable<UserRoleData>>(), null));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(_offerId, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Offer name needs to be set here");
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithWithoutTechnicalUserNeededAndSingleInstance_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId, A<OfferTypeId>._))
            .Returns((false, Enumerable.Empty<IEnumerable<UserRoleData>>(), OfferName));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOffer(_offerId, OfferTypeId.APP);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceProfiles = new IEnumerable<UserRoleData>[] {
            new UserRoleData [] { new (Guid.NewGuid(), "cl1", "test role") }
        };
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId, A<OfferTypeId>._))
            .Returns((false, serviceProfiles, OfferName));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOffer(_offerId, OfferTypeId.APP);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithoutOfferName_ThrowsException()
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerSubscriptionId))
            .Returns((true, Enumerable.Empty<IEnumerable<UserRoleData>>(), null));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerSubscriptionId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Offer name needs to be set here");
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithWithoutTechnicalUserNeededAndSingleInstance_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerSubscriptionId))
            .Returns((false, Enumerable.Empty<IEnumerable<UserRoleData>>(), OfferName));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerSubscriptionId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceProfiles = new IEnumerable<UserRoleData>[] {
            new UserRoleData [] { new (Guid.NewGuid(), "cl1", "test role") }
        };
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerSubscriptionId))
            .Returns((false, serviceProfiles, OfferName));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerSubscriptionId);

        // Assert
        result.Should().HaveCount(1);
    }
}

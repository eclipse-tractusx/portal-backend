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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class TechnicalUserProfileServiceTests
{
    private const string OfferName = "Super App";
    private readonly Guid _offerId = Guid.NewGuid();
    private readonly Guid _offerSubscriptionId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly TechnicalUserProfileSettings _settings;
    private readonly TechnicalUserProfileService _sut;

    public TechnicalUserProfileServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _offerRepository = A.Fake<IOfferRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _settings = new TechnicalUserProfileSettings
        {
            ServiceAccountRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"test", new[] {"role"}}
            }
        };
        _sut = new TechnicalUserProfileService(_portalRepositories, Options.Create(_settings));
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithoutOffer_ThrowsException()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>());
        
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(_offerId).ToListAsync().ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer {_offerId} does not exists");
    }
    
    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithoutOfferName_ThrowsException()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(true, false, null));
        
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(_offerId).ToListAsync().ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Offer name needs to be set here");
    }
    
    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithWithoutTechnicalUserNeededAndSingleInstance_ReturnsExpected()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(false, false, OfferName));
        
        var result = await _sut.GetTechnicalUserProfilesForOffer(_offerId).ToListAsync().ConfigureAwait(false);
        
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_WithValidData_ReturnsExpected()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileData(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(true, true, OfferName));
        A.CallTo(                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new List<UserRoleData>
            {
                new(Guid.NewGuid(), "cl1", "test role")
            }.ToAsyncEnumerable());
        var result = await _sut.GetTechnicalUserProfilesForOffer(_offerId).ToListAsync().ConfigureAwait(false);
        
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithoutOfferName_ThrowsException()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(true, false, null));
        
        async Task Act() => await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerId).ToListAsync().ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Offer name needs to be set here");
    }
    
    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithWithoutTechnicalUserNeededAndSingleInstance_ReturnsExpected()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(false, false, OfferName));
        
        var result = await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerId).ToListAsync().ConfigureAwait(false);
        
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetTechnicalUserProfilesForOfferSubscription_WithValidData_ReturnsExpected()
    {
        A.CallTo(() => _offerRepository.GetServiceAccountProfileDataForSubscription(_offerId))
            .ReturnsLazily(() => new ValueTuple<bool, bool, string?>(true, true, OfferName));
        A.CallTo(                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => new List<UserRoleData>
            {
                new(Guid.NewGuid(), "cl1", "test role")
            }.ToAsyncEnumerable());
        var result = await _sut.GetTechnicalUserProfilesForOfferSubscription(_offerId).ToListAsync().ConfigureAwait(false);
        
        result.Should().HaveCount(1);
    }
}
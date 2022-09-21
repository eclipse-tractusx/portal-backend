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
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared;
using CatenaX.NetworkServices.Tests.Shared.Extensions;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the logic of the <see cref="OfferRepository"/>
/// </summary>
public class OfferRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;

    public OfferRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _contextFake = A.Fake<PortalDbContext>();
    }

    [Fact]
    public async Task GetAllActiveApps_ReturnsReleasedAppsSuccessfully()
    {
        // Arrange
        var apps = _fixture.Build<Offer>()
            .With(a => a.DateReleased, DateTimeOffset.MinValue) // all are active
            .With(a => a.OfferTypeId, OfferTypeId.APP)
            .CreateMany();
        var appsDbSet = apps.AsFakeDbSet();
        var languagesDbSet = new List<Language>().AsFakeDbSet();

        A.CallTo(() => _contextFake.Offers).Returns(appsDbSet);
        A.CallTo(() => _contextFake.Languages).Returns(languagesDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<OfferRepository>();

        // Act
        var results = await sut.GetAllActiveAppsAsync(null).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(apps.Count());
        results.Should().AllBeOfType<AppData>();
        results.Should().AllSatisfy(a => a.Should().Match<AppData>(a => apps.Any(app => app.Id == a.Id)));
    }

    [Fact]
    public async Task GetAppDetails_ReturnsAppDetailsSuccessfully()
    {
        // Arrange
        var apps = _fixture.CreateMany<Offer>(1);
        var appsDbSet = apps.AsFakeDbSet();
        var languagesDbSet = new List<Language>().AsFakeDbSet();

        A.CallTo(() => _contextFake.Offers).Returns(appsDbSet);
        A.CallTo(() => _contextFake.Languages).Returns(languagesDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<OfferRepository>();

        // Act
        var result = await sut.GetAppDetailsByIdAsync(apps.Single().Id, Guid.NewGuid().ToString(), null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<AppDetailsData>();
        result.Id.Should().Be(apps.Single().Id);
    }
}

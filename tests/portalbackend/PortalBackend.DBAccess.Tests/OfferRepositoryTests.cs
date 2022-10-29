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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

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
            .With(a => a.OfferStatusId, OfferStatusId.ACTIVE)
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
        results.Should().AllBeOfType<(Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)>();
        results.Should().AllSatisfy(a => a.Should().Match<(Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)>(a => apps.Any(app => app.Id == a.Id)));
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
        var result = await sut.GetOfferDetailsByIdAsync(apps.Single().Id, Guid.NewGuid().ToString(), null, Constants.DefaultLanguage, OfferTypeId.APP);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OfferDetailsData>();
        result.Should().Match<OfferDetailsData>(r => r.Id == apps.Single().Id);
    }
}

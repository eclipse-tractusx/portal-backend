using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the logic of the <see cref="AppRepository"/>
/// </summary>
public class AppRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;

    public AppRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _contextFake = A.Fake<PortalDbContext>();
    }

    [Fact]
    public async void GetAllActiveApps_ReturnsReleasedAppsSuccessfully()
    {
        // Arrange
        var apps = _fixture.Build<App>()
            .With(a => a.DateReleased, DateTimeOffset.MinValue) // all are active
            .CreateMany();
        var appsDbSet = apps.AsFakeDbSet();
        var languagesDbSet = new List<Language>().AsFakeDbSet();

        A.CallTo(() => _contextFake.Apps).Returns(appsDbSet);
        A.CallTo(() => _contextFake.Languages).Returns(languagesDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<AppRepository>();

        // Act
        var results = await sut.GetAllActiveAppsAsync(null).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(apps.Count());
        results.Should().AllBeOfType<AppData>();
        results.Should().AllSatisfy(a => apps.Select(app => app.Id).Contains(a.Id));
    }
    
    [Fact]
    public async void GetAppDetails_ReturnsAppDetailsSuccessfully()
    {
        // Arrange
        var apps = _fixture.CreateMany<App>(1);
        var appsDbSet = apps.AsFakeDbSet();
        var languagesDbSet = new List<Language>().AsFakeDbSet();

        A.CallTo(() => _contextFake.Apps).Returns(appsDbSet);
        A.CallTo(() => _contextFake.Languages).Returns(languagesDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<AppRepository>();

        // Act
        var result = await sut.GetDetailsByIdAsync(apps.Single().Id, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<AppDetailsData>();
        result.Id.Should().Be(apps.Single().Id);
    }
}

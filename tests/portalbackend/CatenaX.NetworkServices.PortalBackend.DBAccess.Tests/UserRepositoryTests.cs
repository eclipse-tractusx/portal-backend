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
/// Tests the logic of the <see cref="UserRepository"/>
/// </summary>
public class UserRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;

    public UserRepositoryTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _contextFake = A.Fake<PortalDbContext>();
    }

    [Fact]
    public async void GetAllFavouriteAppsForUser_ReturnsAppsSuccessfully()
    {
        // Arrange
        var favouriteApps = _fixture.CreateMany<App>(10);
        var (companyUser, iamUser) = CreateTestUserPair();
        foreach (var app in favouriteApps)
        {
            companyUser.Apps.Add(app);
        }
        var iamUsersFakeDbSet = new List<IamUser>{ iamUser }.AsFakeDbSet();

        A.CallTo(() => _contextFake.IamUsers).Returns(iamUsersFakeDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var results = await sut.GetAllFavouriteAppsForUserUntrackedAsync(iamUser.UserEntityId).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(favouriteApps.Count());
        results.Should().AllSatisfy(a => favouriteApps.Select(app => app.Id).Contains(a));
    }
    
    [Fact]
    public async void GetBusinessApps_ReturnsAppListSuccessfully()
    {
        // Arrange
        var expectedApp = _fixture.Create<PortalBackend.PortalEntities.Entities.App>();
        var (companyUser, iamUser) = CreateTestUserPair();
        companyUser.Company!.BoughtApps.Add(expectedApp);
        foreach (var app in _fixture.CreateMany<PortalBackend.PortalEntities.Entities.App>())
        {
            companyUser.Company.BoughtApps.Add(app);
        }

        var iamClient = _fixture.Create<IamClient>();
        iamClient.Apps.Add(expectedApp);
        foreach (var app in _fixture.CreateMany<PortalBackend.PortalEntities.Entities.App>())
        {
            iamClient.Apps.Add(app);
        }

        foreach (var role in _fixture.Build<UserRole>().With(r => r.IamClient, iamClient).CreateMany())
        {
            companyUser.UserRoles.Add(role);
        }

        var iamUserFakeDbSet = new List<IamUser>() { iamUser }.AsFakeDbSet();

        A.CallTo(() => _contextFake.IamUsers).Returns(iamUserFakeDbSet);
        _fixture.Inject(_contextFake);

        var sut = _fixture.Create<UserRepository>();

        // Act
        var result = await sut.GetAllBusinessAppDataForUserIdAsync(iamUser.UserEntityId).ToListAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(expectedApp.Id);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return (companyUser, iamUser);
    }
}

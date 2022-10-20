using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests;

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

        // Act
        var results = sut.AttachAndModifyOfferSubscription(new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd"),
            sub =>
            {
                sub.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
                sub.DisplayName = "Modified Name";
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.PENDING);
        results.DisplayName.Should().Be("Modified Name");
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<OfferSubscription>().Which.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.PENDING);
    }

    #endregion

    #region GetOfferSubscriptionStateForCompany

    [Fact]
    public async Task GetOfferSubscriptionStateForCompanyAsync_WithExistingData_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferSubscriptionStateForCompanyAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), 
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), 
            OfferTypeId.APP).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe(default);
        result.offerSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        result.offerSubscriptionId.Should().Be(new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd"));
    }

    [Fact]
    public async Task GetOfferSubscriptionStateForCompanyAsync_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetOfferSubscriptionStateForCompanyAsync(
            new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), 
            new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), 
            OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
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
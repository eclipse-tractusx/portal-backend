using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ApplicationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    
    private readonly static Guid ApplicationId = new("4829b64c-de6a-426c-81fc-c0bcf95bcb76");

    public ApplicationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }
    
    #region HasBpnAlreadySet

    [Fact]
    public async Task GetBpnForApplicationIdAsync_ReturnsBpn()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var bpn = await sut.GetBpnForApplicationIdAsync(ApplicationId).ConfigureAwait(false);

        // Assert
        bpn.Should().Be("CAXSDUMMYCATENAZZ");
    }

    #endregion

    private async Task<ApplicationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        return new ApplicationRepository(context);
    }
}
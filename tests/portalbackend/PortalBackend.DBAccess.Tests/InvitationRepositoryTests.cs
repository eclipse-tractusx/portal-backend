using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class InvitationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _applicationId = new("6b2d1263-c073-4a48-bfaf-704dc154ca9e");

    public InvitationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    [Fact]
    public async Task GetInvitedUserDetailsUntrackedAsync_WithValid_ReturnsExpected()
    {
        var sut = await CreateSut().ConfigureAwait(false);

        var result = await sut.GetInvitedUserDetailsUntrackedAsync(_applicationId).ToListAsync().ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Satisfy(
            x => x.EmailId == "test@user.com" && x.InvitationStatus == InvitationStatusId.CREATED,
            x => x.EmailId == "company.admin1@acme.corp" && x.InvitationStatus == InvitationStatusId.CREATED);
    }

    #region Setup    

    private async Task<InvitationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new InvitationRepository(context);
        return sut;
    }

    #endregion
}

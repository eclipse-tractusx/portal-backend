using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanyInvitationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _processId = new("70f0f368-5058-4aca-808b-cece869bcef2");
    private readonly Guid _invitationId = new("32705785-b056-4f36-9a71-71b795344bb2");
    public CompanyInvitationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateCompanyInvitation

    [Fact]
    public async Task CreateCompanyInvitation_WithValidData_Creates()
    {
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);
        var processId = Guid.NewGuid();

        var invitation = sut.CreateCompanyInvitation("tony", "stark", "tony@stark.com", "stark industry", processId, x =>
            {
                x.UserName = "ironman";
            });

        // Assert
        invitation.Id.Should().NotBeEmpty();
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyInvitation>()
            .Which.UserName.Should().Be("ironman");
    }

    #endregion

    #region GetCompanyInvitationForProcessId

    [Fact]
    public async Task GetCompanyInvitationForProcessId_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyInvitationForProcessId(_processId).ConfigureAwait(false);

        // Assert
        data.Should().Be(_invitationId);
    }

    [Fact]
    public async Task GetCompanyInvitationForProcessId_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetCompanyInvitationForProcessId(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetInvitationIdpData

    [Fact]
    public async Task GetInvitationIdpData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationIdpData(_invitationId).ConfigureAwait(false);

        // Assert
        data.Should().Be("stark industry");
    }

    [Fact]
    public async Task GetInvitationIdpData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationIdpData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.Should().BeNull();
    }

    #endregion

    #region GetInvitationUserData

    [Fact]
    public async Task GetInvitationUserData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationUserData(_invitationId).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeTrue();
        data.userInformation.Email.Should().Be("tony@stark.com");
        data.userInformation.FirstName.Should().Be("tony");
    }

    [Fact]
    public async Task GetInvitationUserData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationUserData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeFalse();
    }

    #endregion

    #region GetInvitationIdpCreationData

    [Fact]
    public async Task GetInvitationIdpCreationData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationIdpCreationData(_invitationId).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeTrue();
        data.orgName.Should().Be("stark industry");
        data.idpName.Should().Be("test idp");
    }

    [Fact]
    public async Task GetInvitationIdpCreationData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetInvitationIdpCreationData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeFalse();
    }

    #endregion

    #region AttachAndModifyCompanyInvitation

    [Fact]
    public async Task AttachAndModifyCompanyInvitation()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);
        var existingId = Guid.NewGuid();

        // Act
        sut.AttachAndModifyCompanyInvitation(existingId, invitation => { invitation.IdpName = null; }, invitation => { invitation.IdpName = "test"; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle().And.AllSatisfy(x => x.Entity.Should().BeOfType<CompanyInvitation>()).And.Satisfy(
            x => x.State == EntityState.Modified && ((CompanyInvitation)x.Entity).Id == existingId && ((CompanyInvitation)x.Entity).IdpName == "test"
        );
    }

    #endregion

    #region GetMailData

    [Fact]
    public async Task GetMailData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetMailData(_invitationId).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeTrue();
        data.orgName.Should().Be("stark industry");
        data.email.Should().Be("tony@stark.com");
    }

    [Fact]
    public async Task GetMailData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetMailData(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        data.exists.Should().BeFalse();
    }

    #endregion

    #region Setup

    private async Task<(ICompanyInvitationRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyInvitationRepository(context);
        return (sut, context);
    }

    private async Task<ICompanyInvitationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyInvitationRepository(context);
        return sut;
    }

    #endregion
}

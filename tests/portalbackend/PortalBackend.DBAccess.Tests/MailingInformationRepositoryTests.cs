using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class MailingInformationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _processId = new("44927361-3766-4f07-9f18-860158880d86");
    public MailingInformationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetMailingInformationForProcess

    [Fact]
    public async Task GetMailingInformationForProcess_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetMailingInformationForProcess(_processId).ToListAsync().ConfigureAwait(false);

        // Assert
        data.Should().ContainSingle().And.Satisfy(x => x.Template == "CredentialRejected" && x.EmailAddress == "test@email.de");
    }

    [Fact]
    public async Task GetMailingInformationForProcess_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var data = await sut.GetMailingInformationForProcess(Guid.NewGuid()).ToListAsync().ConfigureAwait(false);

        // Assert
        data.Should().BeEmpty();
    }

    #endregion

    #region CreateMailingInformation

    [Fact]
    public async Task CreateMailingInformation_WithValidData_Creates()
    {
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        var mailingInformation = sut.CreateMailingInformation(_processId, "test@email.de", "test mail", new Dictionary<string, string>());

        // Assert
        mailingInformation.Id.Should().NotBeEmpty();
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<MailingInformation>()
            .Which.Email.Should().Be("test@email.de");
    }

    #endregion

    #region AttachAndModifyMailingInformation

    [Fact]
    public async Task AttachAndModifyMailingInformation()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);
        var existingId = Guid.NewGuid();

        // Act
        sut.AttachAndModifyMailingInformation(existingId, information => { information.MailingStatusId = MailingStatusId.Pending; }, information => { information.MailingStatusId = MailingStatusId.Send; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle().And.AllSatisfy(x => x.Entity.Should().BeOfType<MailingInformation>()).And.Satisfy(
            x => x.State == EntityState.Modified && ((MailingInformation)x.Entity).Id == existingId && ((MailingInformation)x.Entity).MailingStatusId == MailingStatusId.Send
        );
    }

    #endregion

    #region Setup

    private async Task<(IMailingInformationRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new MailingInformationRepository(context);
        return (sut, context);
    }

    private async Task<IMailingInformationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new MailingInformationRepository(context);
        return sut;
    }

    #endregion
}

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class ProcessStepRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ProcessStepRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateProcess

    [Fact]
    public async Task CreateProcess_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcess(ProcessTypeId.APPLICATION_CHECKLIST);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<Process>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<Process>()
            .Should().Satisfy(
                x => x.Id == result.Id && x.ProcessTypeId == ProcessTypeId.APPLICATION_CHECKLIST
            );
    }

    #endregion

    #region CreateProcessStep

    [Fact]
    public async Task CreateProcessStep_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcessStep(ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.TODO, new Guid("c9aaa06c-15ad-4de7-8e42-ab3b5f4a74d4"));

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<ProcessStep>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<ProcessStep>()
            .Should().Satisfy(
                x => x.Id == result.Id && x.ProcessStepTypeId == ProcessStepTypeId.ACTIVATE_APPLICATION && x.ProcessStepStatusId == ProcessStepStatusId.TODO && x.ProcessId == new Guid("c9aaa06c-15ad-4de7-8e42-ab3b5f4a74d4"));
    }

    #endregion

    #region CreateProcessStepRange

    [Fact]

    public async Task CreateProcessStepRange_CreateSuccessfully()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3).ToImmutableArray();
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcessStepRange(processId, processStepTypeIds);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .HaveSameCount(processStepTypeIds)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<ProcessStep>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<ProcessStep>()
            .Should().Satisfy(
                x => x.Id == result.ElementAt(0).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[0] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(1).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[1] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(2).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[2] && x.ProcessStepStatusId == ProcessStepStatusId.TODO
            );
    }

    #endregion

    #region AttachAndModifyProcessStep

    [Fact]
    public async Task AttachAndModifyProcessStep_WithExistingProcessStep_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyProcessStep(new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"),
            existing =>
            {
                existing.ProcessStepStatusId = ProcessStepStatusId.TODO;
            },
            modify =>
            {
                modify.ProcessStepStatusId = ProcessStepStatusId.DONE;
            }
        );

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<ProcessStep>().Which.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion

    #region GetActiveProcesses

    [Fact]
    public async Task GetActiveProcess_ReturnsExpected()
    {
        // Arrange
        var processTypeIds = new [] { ProcessTypeId.APPLICATION_CHECKLIST };
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetActiveProcesses(processTypeIds).ToListAsync().ConfigureAwait(false);
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.ProcessId == new Guid("1f9a3232-9772-4ecb-8f50-c16e97772dfe") && x.ProcessTypeId == ProcessTypeId.APPLICATION_CHECKLIST
            );
    }

    #endregion

    #region GetProcessStepData

    [Fact]
    public async Task GetProcessStepData_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("1f9a3232-9772-4ecb-8f50-c16e97772dfe");
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProcessStepData(processId).ToListAsync().ConfigureAwait(false);
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.ProcessStepId == new Guid("2d03703e-8f10-4e8e-a194-f04d0ae15c35") && x.ProcessStepTypeId == ProcessStepTypeId.START_CLEARING_HOUSE
            );
    }

    #endregion

    private async Task<(ProcessStepRepository sut, PortalDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ProcessStepRepository(context);
        return (sut, context);
    }

    private async Task<ProcessStepRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new ProcessStepRepository(context);
        return sut;
    }
}
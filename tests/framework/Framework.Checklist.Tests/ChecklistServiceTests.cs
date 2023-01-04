using Org.Eclipse.TractusX.Portal.Backend.Framework.Checklist;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Framework.Checklist.Tests;

public class ChecklistServiceTests
{
    private static readonly Guid ApplicationWithoutBpnId = new ("0a9bd7b1-e692-483e-8128-dbf52759c7a5");
    private static readonly Guid ApplicationWithBpnId = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IApplicationChecklistRepository _applicationChecklistRepository;
    private readonly ChecklistService _service;

    public ChecklistServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        
        _applicationRepository = A.Fake<IApplicationRepository>();
        _applicationChecklistRepository = A.Fake<IApplicationChecklistRepository>();

        _service = new ChecklistService(_portalRepositories);
    }
    
    #region CreateInitialChecklistAsync

    [Fact]
    public async Task CreateInitialChecklistAsync_WithBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
            ApplicationWithBpnId,
            A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                .That
                .Matches(x => 
                    x.Count(y => y.TypeId == ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER && y.StatusId == ChecklistEntryStatusId.DONE) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateInitialChecklistAsync_WithoutBpnSet_CreatesExpectedResult()
    {
        // Arrange
        SetupFakesForCreate();
        
        // Act
        await _service.CreateInitialChecklistAsync(ApplicationWithoutBpnId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _applicationChecklistRepository.CreateChecklistForApplication(
                ApplicationWithoutBpnId,
                A<IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>
                    .That
                    .Matches(x => x.All(y => y.StatusId == ChecklistEntryStatusId.TO_DO))))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region UpdateBpnStatus
    
    [Theory]
    [InlineData(ChecklistEntryStatusId.IN_PROGRESS)]
    [InlineData(ChecklistEntryStatusId.DONE)]
    [InlineData(ChecklistEntryStatusId.FAILED)]
    public async Task UpdateBpnStatus_WithStatus_SetsExpectedStatus(ChecklistEntryStatusId statusId)
    {
        // Arrange
        var entry = new ApplicationChecklistEntry(ApplicationWithBpnId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow);
        SetupFakesForUpdate(entry);
        
        // Act
        await _service.UpdateBpnStatus(ApplicationWithoutBpnId, statusId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        entry.StatusId = statusId;
    }
    
    #endregion

    #region Setup

    private void SetupFakesForCreate()
    {
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithBpnId)).ReturnsLazily(() => "testbpn");
        A.CallTo(() => _applicationRepository.GetBpnForApplicationIdAsync(ApplicationWithoutBpnId)).ReturnsLazily(() => (string?)null);

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    private void SetupFakesForUpdate(ApplicationChecklistEntry applicationChecklistEntry)
    {
        A.CallTo(() => _applicationChecklistRepository.AttachAndModifyApplicationChecklist(A<Guid>._, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, A<Action<ApplicationChecklistEntry>>._))
            .Invokes((Guid _, ChecklistEntryTypeId _, Action<ApplicationChecklistEntry> setFields) =>
                {
                    applicationChecklistEntry.DateLastChanged = DateTimeOffset.UtcNow;
                    setFields.Invoke(applicationChecklistEntry);
                });

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationChecklistRepository>()).Returns(_applicationChecklistRepository);
    }

    #endregion
}
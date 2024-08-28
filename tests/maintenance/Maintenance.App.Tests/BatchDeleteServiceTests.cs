using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Maintenance.App.Tests;

public class BatchDeleteServiceTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IDocumentRepository _documentRepository;
    private readonly BatchDeleteService _sut;
    private readonly IMockLogger<BatchDeleteService> _mockLogger;

    public BatchDeleteServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockLogger = A.Fake<IMockLogger<BatchDeleteService>>();
        ILogger<BatchDeleteService> logger = new MockLogger<BatchDeleteService>(_mockLogger);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _documentRepository = A.Fake<IDocumentRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);

        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);

        var options = Options.Create(new BatchDeleteServiceSettings { DeleteIntervalInDays = 5 });
        _sut = new BatchDeleteService(logger, options, _portalRepositories, dateTimeProvider);
    }

    [Fact]
    public async Task CleanupDocuments_WithoutMatchingDocuments_DoesNothing()
    {
        // Arrange
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Returns([]);

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.AttachRange(A<IEnumerable<Agreement>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveOfferAssignedDocuments(A<IEnumerable<OfferAssignedDocument>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CleanupDocuments_WithMatchingDocuments_RemovesExpected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var agreementId1 = Guid.NewGuid();
        var agreementId2 = Guid.NewGuid();
        var offerId1 = Guid.NewGuid();
        var offerId2 = Guid.NewGuid();
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Returns([new(documentId, new[] { agreementId1, agreementId2 }, new[] { offerId1, offerId2 })]);

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.AttachRange(A<IEnumerable<Agreement>>.That.Matches(x => x.Count(a => a.Id == agreementId1) == 1 && x.Count(a => a.Id == agreementId2) == 1)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>.That.Matches(x => x.Single() == documentId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveOfferAssignedDocuments(A<IEnumerable<OfferAssignedDocument>>.That.Matches(x => x.Count(a => a.OfferId == offerId1 && a.DocumentId == documentId) == 1 && x.Count(a => a.OfferId == offerId2 && a.DocumentId == documentId) == 1)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CleanupDocuments_WithException_LogsException()
    {
        // Arrange
        A.CallTo(() => _documentRepository.GetDocumentDataForCleanup(A<DateTimeOffset>._))
            .Throws(new DataMisalignedException("Test message"));

        // Act
        await _sut.CleanupDocuments(CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.AttachRange(A<IEnumerable<Agreement>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.RemoveOfferAssignedDocuments(A<IEnumerable<OfferAssignedDocument>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _mockLogger.Log(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._, A<string>._)).MustHaveHappenedOnceExactly();
    }
}

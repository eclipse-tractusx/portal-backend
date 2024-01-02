using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.VerifiedCredential.App;
using Org.Eclipse.TractusX.Portal.Backend.VerifiedCredential.App.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.VerifiedCredentials.App.Tests;

public class ExpiryCheckServiceTests
{
    private readonly IFixture _fixture;
    private readonly ExpiryCheckService _sut;

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMailingService _mailingService;
    private readonly IProcessIdentityDataDetermination _processIdentityDataDetermination;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ExpiryCheckServiceSettings _settings;

    public ExpiryCheckServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanySsiDetailsRepository>())
            .Returns(_companySsiDetailsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>())
            .Returns(_notificationRepository);

        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _mailingService = A.Fake<IMailingService>();
        _processIdentityDataDetermination = A.Fake<IProcessIdentityDataDetermination>();

        var serviceProvider = _fixture.Create<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalRepositories))).Returns(_portalRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IDateTimeProvider))).Returns(_dateTimeProvider);
        A.CallTo(() => serviceProvider.GetService(typeof(IMailingService))).Returns(_mailingService);
        A.CallTo(() => serviceProvider.GetService(typeof(IProcessIdentityDataDetermination))).Returns(_processIdentityDataDetermination);
        var serviceScope = _fixture.Create<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = _fixture.Create<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);

        _settings = new ExpiryCheckServiceSettings
        {
            ExpiredVcsToDeleteInMonth = 12,
            InactiveVcsToDeleteInWeeks = 8
        };
        _sut = new ExpiryCheckService(serviceScopeFactory, _fixture.Create<ILogger<ExpiryCheckService>>(), Options.Create(_settings));
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveAndEligibleForDeletion_RemovesEntry()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var inactiveVcsToDelete = now.AddDays(-(_settings.InactiveVcsToDeleteInWeeks * 7));
        var credentialId = Guid.NewGuid();
        var data = new CredentialExpiryData[]
        {
            new(credentialId, inactiveVcsToDelete.AddDays(-1), now.AddMonths(12), null, null, CompanySsiDetailStatusId.INACTIVE, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, _fixture.Create<UserMailingData>())
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(credentialId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.ACTIVE)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task ExecuteAsync_WithExpiredEligibleForDeletion_RemovesEntry(CompanySsiDetailStatusId statusId)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var inactiveVcsToDelete = now.AddDays(-(_settings.InactiveVcsToDeleteInWeeks * 7));
        var expiredVcsToDeleteInMonth = now.AddMonths(-_settings.ExpiredVcsToDeleteInMonth);
        var credentialId = Guid.NewGuid();
        var data = new CredentialExpiryData[]
        {
            new(credentialId, inactiveVcsToDelete.AddDays(3), expiredVcsToDeleteInMonth.AddDays(-3), null, null, statusId, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, _fixture.Create<UserMailingData>())
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(credentialId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("test@example.org")]
    public async Task ExecuteAsync_WithPendingAndExpiryBeforeNow_DeclinesRequest(string? email)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expiredVcsToDeleteInMonth = now.AddMonths(-_settings.ExpiredVcsToDeleteInMonth);
        var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), Guid.NewGuid(), VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), Guid.NewGuid(), now)
        {
            ExpiryDate = expiredVcsToDeleteInMonth.AddDays(-2)
        };
        var userId = Guid.NewGuid();
        var userMailingData = new UserMailingData(userId, email, "Test", "User");
        var data = new CredentialExpiryData[]
        {
            new(ssiDetail.Id, ssiDetail.DateCreated, ssiDetail.ExpiryDate.Value, ssiDetail.ExpiryCheckTypeId, null, ssiDetail.CompanySsiDetailStatusId, ssiDetail.VerifiedCredentialTypeId, userMailingData)
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(A<Guid>._,
                A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(ssiDetail);
                updateFields.Invoke(ssiDetail);
            });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(userId, NotificationTypeId.CREDENTIAL_REJECTED, false, A<Action<Notification>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(ssiDetail.Id)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        if (string.IsNullOrWhiteSpace(email))
        {
            A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Contains("CredentialRejected"))).MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _mailingService.SendMails(email, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Contains("CredentialRejected"))).MustHaveHappenedOnceExactly();
        }

        ssiDetail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.INACTIVE);
    }

    [Theory]
    [InlineData(null, 1, ExpiryCheckTypeId.OneDay, ExpiryCheckTypeId.TwoWeeks)]
    [InlineData("test@example.org", 1, ExpiryCheckTypeId.OneDay, ExpiryCheckTypeId.TwoWeeks)]
    [InlineData(null, 13, ExpiryCheckTypeId.TwoWeeks, ExpiryCheckTypeId.OneMonth)]
    [InlineData("test@example.org", 13, ExpiryCheckTypeId.TwoWeeks, ExpiryCheckTypeId.OneMonth)]
    [InlineData(null, 27, ExpiryCheckTypeId.OneMonth, null)]
    [InlineData("test@example.org", 27, ExpiryCheckTypeId.OneMonth, null)]
    public async Task ExecuteAsync_WithActiveCloseToExpiry_NotifiesCreator(string? email, int days, ExpiryCheckTypeId expiryCheckTypeId, ExpiryCheckTypeId? currentExpiryCheckTypeId)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), Guid.NewGuid(), VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, CompanySsiDetailStatusId.ACTIVE, Guid.NewGuid(), Guid.NewGuid(), now)
        {
            ExpiryDate = now.AddDays(-days),
            ExpiryCheckTypeId = currentExpiryCheckTypeId
        };
        var userId = Guid.NewGuid();
        var userMailingData = new UserMailingData(userId, email, "Test", "User");
        var data = new CredentialExpiryData[]
        {
            new(ssiDetail.Id, ssiDetail.DateCreated, ssiDetail.ExpiryDate.Value, ssiDetail.ExpiryCheckTypeId, null, ssiDetail.CompanySsiDetailStatusId, ssiDetail.VerifiedCredentialTypeId, userMailingData)
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(A<Guid>._,
                A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(ssiDetail);
                updateFields.Invoke(ssiDetail);
            });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(userId, NotificationTypeId.CREDENTIAL_EXPIRY, false, A<Action<Notification>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(ssiDetail.Id)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        if (string.IsNullOrWhiteSpace(email))
        {
            A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Contains("CredentialExpiry"))).MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _mailingService.SendMails(email, A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Contains("CredentialExpiry"))).MustHaveHappenedOnceExactly();
        }

        ssiDetail.ExpiryCheckTypeId.Should().Be(expiryCheckTypeId);
    }
}

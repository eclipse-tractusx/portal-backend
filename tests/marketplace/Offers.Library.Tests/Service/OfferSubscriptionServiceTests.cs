/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSubscriptionServiceTests
{
    private const string BasePortalUrl = "http//base-url.com";
    private const string ClientId = "Client1";
    private readonly Guid _salesManagerId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");

    private readonly string _notAssignedCompanyIdUser;
    private readonly string _noBpnSetUserId;
    private readonly string _iamUserId;
    private readonly string _existingActiveSubscriptionUserId;
    private readonly string _existingInactiveSubscriptionUserId;
    private readonly Guid _companyUserId;
    private readonly Guid _companyId;
    private readonly Guid _existingActiveSubscriptionCompanyId;
    private readonly Guid _existingInactiveSubscriptionCompanyId;
    private readonly Guid _existingOfferId;
    private readonly Guid _existingOfferIdWithoutProviderEmail;
    private readonly Guid _existingOfferWithFailingAutoSetupId;
    private readonly Guid _existingOfferWithoutDetailsFilled;
    private readonly Guid _validSubscriptionId;
    private readonly Guid _newOfferSubscriptionId;
    private readonly Guid _userRoleId;
    private readonly IEnumerable<Guid> _offerAgreementIds;
    private readonly IEnumerable<OfferAgreementConsentData> _validConsentData;
    private readonly IFixture _fixture;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IConsentAssignedOfferSubscriptionRepository _consentAssignedOfferSubscriptionRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IMailingService _mailingService;
    private readonly Dictionary<string, IEnumerable<string>> _serviceManagerRoles;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly OfferSubscriptionService _sut;

    public OfferSubscriptionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _iamUserId = _fixture.Create<string>();
        _existingActiveSubscriptionUserId = _fixture.Create<string>();
        _existingInactiveSubscriptionUserId = _fixture.Create<string>();
        _companyUserId = _fixture.Create<Guid>();
        _companyId = _fixture.Create<Guid>();
        _existingOfferIdWithoutProviderEmail = _fixture.Create<Guid>();
        _existingActiveSubscriptionCompanyId = _fixture.Create<Guid>();
        _existingInactiveSubscriptionCompanyId = _fixture.Create<Guid>();
        _notAssignedCompanyIdUser = _fixture.Create<string>();
        _noBpnSetUserId = _fixture.Create<string>();
        _existingOfferId = _fixture.Create<Guid>();
        _existingOfferWithFailingAutoSetupId = _fixture.Create<Guid>();
        _existingOfferWithoutDetailsFilled = _fixture.Create<Guid>();
        _validSubscriptionId = _fixture.Create<Guid>();
        _newOfferSubscriptionId = _fixture.Create<Guid>();
        _userRoleId = _fixture.Create<Guid>();
        _offerAgreementIds = _fixture.CreateMany<Guid>().ToImmutableArray();
        _validConsentData = _offerAgreementIds.Select(x => new OfferAgreementConsentData(x, ConsentStatusId.ACTIVE));
        _serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, new[] { "Service Manager" } }
        };

        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _consentAssignedOfferSubscriptionRepository = A.Fake<IConsentAssignedOfferSubscriptionRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _mailingService = A.Fake<IMailingService>();

        SetupRepositories();

        _sut = new OfferSubscriptionService(_portalRepositories, _mailingService);
    }

    #region Add Offer Subscription

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithExistingId_CreatesServiceSubscription(OfferTypeId offerTypeId)
    {
        // Arrange
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId) =>
            {
                var companyAssignedApp = new OfferSubscription(_newOfferSubscriptionId, offerId, companyId, offerSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        var notificationId = Guid.NewGuid();
        var notifications = new List<Notification>();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });

        // Act
        await _sut.AddOfferSubscriptionAsync(_existingOfferId, _validConsentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        notifications.Should().HaveCount(2);
        notifications.Should().OnlyHaveUniqueItems();
        notifications.Should().AllSatisfy(x => x.Done.Should().NotBeNull().And.BeFalse());
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x => x.Count() == 1 && x.Single().ProcessStepTypeId == ProcessStepTypeId.TRIGGER_PROVIDER))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithSalesManagerEqualsReceiver_CreatesServiceSubscription(OfferTypeId offerTypeId)
    {
        // Arrange
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId) =>
            {
                var companyAssignedApp = new OfferSubscription(_newOfferSubscriptionId, offerId, companyId, offerSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        var notificationId = Guid.NewGuid();
        var notifications = new List<Notification>();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });

        // Act
        await _sut.AddOfferSubscriptionAsync(_existingOfferId, _validConsentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        notifications.Should().HaveCount(2);
        notifications.Should().OnlyHaveUniqueItems();
        notifications.Should().AllSatisfy(x => x.Done.Should().NotBeNull().And.BeFalse());
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithFailingAutoSetup_ReturnsExpectedResult(OfferTypeId offerTypeId)
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notifications = new List<Notification>();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });

        // Act
        await _sut.AddOfferSubscriptionAsync(_existingOfferWithFailingAutoSetupId, _validConsentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        notifications.Should().ContainSingle();
        notifications.First().Content.Should().Contain("Error occured");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithExistingIdWithoutProviderEmail_CreatesServiceSubscription(OfferTypeId offerTypeId)
    {
        // Arrange 
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid creatorId) =>
            {
                var companyAssignedApp = new OfferSubscription(_newOfferSubscriptionId, offerId, companyId, offerSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });

        // Act
        await _sut.AddOfferSubscriptionAsync(_existingOfferIdWithoutProviderEmail, new List<OfferAgreementConsentData>(), _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x => x.Count() == 1 && x.Single().ProcessStepTypeId == ProcessStepTypeId.TRIGGER_PROVIDER))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_NotAssignedCompany_ThrowsException(OfferTypeId offerTypeId)
    {
        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _notAssignedCompanyIdUser, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
        ex.Message.Should().Be($"User {_notAssignedCompanyIdUser} has no company assigned (Parameter 'iamUserId')");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithNotExistingId_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(notExistingServiceId, new List<OfferAgreementConsentData>(), _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Offer {notExistingServiceId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_NotAssignedCompanyUser_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var invalidUser = _fixture.Create<string>();
        ;

        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), invalidUser, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
        ex.Message.Should().Be($"User {invalidUser} has no company user assigned (Parameter 'iamUserId')");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithoutOfferProviderDetails_ThrowsException(OfferTypeId offerTypeId)
    {
        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferWithoutDetailsFilled, new List<OfferAgreementConsentData>(), _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be("The offer name has not been configured properly");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithMissingConsentData_ThrowsControllerArgumentException(OfferTypeId offerTypeId)
    {
        // Arrange
        var consentData = Enumerable.Empty<OfferAgreementConsentData>();

        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, consentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("offerAgreementConsentData");
        ex.Message.Should().EndWith($"must be given for offer {_existingOfferId} (Parameter 'offerAgreementConsentData')");
        ex.Message.Should().ContainAll(_offerAgreementIds.Select(id => id.ToString()));
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithAdditionalConsentData_ThrowsControllerArgumentException(OfferTypeId offerTypeId)
    {
        // Arrange
        var additional = _fixture.CreateMany<OfferAgreementConsentData>().ToImmutableArray();
        var consentData = _validConsentData.Concat(additional).ToImmutableArray();

        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, consentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("offerAgreementConsentData");
        ex.Message.Should().EndWith($"are not valid for offer {_existingOfferId} (Parameter 'offerAgreementConsentData')");
        ex.Message.Should().ContainAll(additional.Select(consent => consent.AgreementId.ToString()));
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithInactiveConsentData_ThrowsControllerArgumentException(OfferTypeId offerTypeId)
    {
        // Arrange
        var consentData = _offerAgreementIds.Select(id => new OfferAgreementConsentData(id, ConsentStatusId.INACTIVE)).ToImmutableArray();

        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, consentData, _iamUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("offerAgreementConsentData");
        ex.Message.Should().EndWith($"must be given for offer {_existingOfferId} (Parameter 'offerAgreementConsentData')");
        ex.Message.Should().ContainAll(_offerAgreementIds.Select(id => id.ToString()));
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithoutBuisnessPartnerNumber_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Act
        async Task Action() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _noBpnSetUserId, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Contain("has no BusinessPartnerNumber assigned");
    }

    #endregion

    #region APP - Specialcases

    [Fact]
    public async Task AddOfferSubscription_WithExistingActiveSubscription_ThrowsConflictException()
    {
        // Arrange 
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionStateForCompanyAsync(_existingOfferId, _existingActiveSubscriptionCompanyId, A<OfferTypeId>._))
            .Returns(new ValueTuple<Guid, OfferSubscriptionStatusId>(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE));

        // Act
        async Task Act() => await _sut.AddOfferSubscriptionAsync(_existingOfferId, _validConsentData, _existingActiveSubscriptionUserId, OfferTypeId.APP, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Contain(" is already subscribed to ");
    }

    [Fact]
    public async Task AddOfferSubscription_WithExistingInactiveSubscription_UpdatesState()
    {
        // Arrange
        var offerSubscription = new OfferSubscription(Guid.NewGuid(), _existingOfferId, _companyId, OfferSubscriptionStatusId.INACTIVE, Guid.NewGuid(), Guid.NewGuid());
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionStateForCompanyAsync(_existingOfferId, _existingInactiveSubscriptionCompanyId, A<OfferTypeId>._))
            .Returns(new ValueTuple<Guid, OfferSubscriptionStatusId>(offerSubscription.Id, offerSubscription.OfferSubscriptionStatusId));
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(offerSubscription.Id, A<Action<OfferSubscription>>._))
            .Invokes((Guid _, Action<OfferSubscription> setOptionalFields) =>
            {
                setOptionalFields.Invoke(offerSubscription);
            });

        // Act
        await _sut.AddOfferSubscriptionAsync(_existingOfferId, _validConsentData, _existingInactiveSubscriptionUserId, OfferTypeId.APP, BasePortalUrl).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._, A<Action<OfferSubscription>>._)).MustHaveHappenedOnceExactly();
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.PENDING);
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        var offerDetailData = _fixture.CreateMany<ServiceOverviewData>(5);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<ServiceOverviewData>(offerDetailData.Count(), offerDetailData.Skip(skip).Take(take)));
        var offerDetail = _fixture.Build<OfferDetailData>()
            .With(x => x.Id, _existingOfferId)
            .Create();

        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_iamUserId))
            .Returns((
                new CompanyInformationData(_companyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_notAssignedCompanyIdUser))
            .Returns((
                new CompanyInformationData(Guid.Empty, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_noBpnSetUserId))
            .Returns((
                new CompanyInformationData(_companyId, "The Company", "DE", null),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_existingActiveSubscriptionUserId))
            .Returns((
                new CompanyInformationData(_existingActiveSubscriptionCompanyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_existingInactiveSubscriptionUserId))
            .Returns((
                new CompanyInformationData(_existingInactiveSubscriptionCompanyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(A<string>.That.Not.Matches(x => x == _iamUserId || x == _notAssignedCompanyIdUser || x == _noBpnSetUserId || x == _existingActiveSubscriptionUserId || x == _existingInactiveSubscriptionUserId)))
            .Returns((
                new CompanyInformationData(_companyId, "The Company", "DE", "BPM00000001"),
                Guid.Empty,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetServiceProviderCompanyUserWithRoleIdAsync(A<Guid>.That.Matches(x => x == _existingOfferId), A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == _userRoleId))))
            .Returns(new List<Guid> { _companyUserId, _salesManagerId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid> { _userRoleId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Not.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _offerRepository.GetActiveServicesPaginationSource(A<ServiceOverviewSorting>._, A<ServiceTypeId>._, A<string>._))
            .Returns(paginationResult);
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(_existingOfferId, A<string>.That.Matches(x => x == "en"), A<string>._, A<OfferTypeId>._))
            .Returns(offerDetail with
            {
                OfferSubscriptionDetailData = new[]
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }
            });
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingOfferId), A<string>._, A<string>._, A<OfferTypeId>._))
            .Returns((OfferDetailData?)null);

        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferId), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData("Test Offer", "Test Company", "provider@mail.de", _salesManagerId, "https://www.testurl.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferIdWithoutProviderEmail), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData("Test Offer", "Test Company", null, _salesManagerId, "https://www.testurl.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferWithFailingAutoSetupId), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData("Test Offer", "Test Company", "provider@mail.de", _salesManagerId, "https://www.fail.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferWithoutDetailsFilled), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData(null, "Test Company", null, _salesManagerId, "https://www.fail.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingOfferId || x == _existingOfferWithFailingAutoSetupId || x == _existingOfferWithoutDetailsFilled || x == _existingOfferIdWithoutProviderEmail), A<OfferTypeId>._))
            .Returns((OfferProviderDetailsData?)null);

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .Returns(new SubscriptionDetailData(_existingOfferId, "Super Offer", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .Returns((SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingOfferId), A<string>.That.Matches(x => x == _iamUserId), A<OfferTypeId>._))
            .Returns((_companyId, offerSubscription, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingOfferId), A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .Returns((_companyId, null, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingOfferId), A<string>.That.Not.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .Returns(((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        A.CallTo(() => _agreementRepository.GetAgreementIdsForOfferAsync(A<Guid>.That.Matches(id => id == _existingOfferId || id == _existingOfferWithFailingAutoSetupId || id == _existingOfferWithoutDetailsFilled)))
            .Returns(_offerAgreementIds.ToAsyncEnumerable());

        A.CallTo(() => _agreementRepository.GetAgreementIdsForOfferAsync(A<Guid>.That.Not.Matches(id => id == _existingOfferId || id == _existingOfferWithFailingAutoSetupId || id == _existingOfferWithoutDetailsFilled)))
            .Returns(_fixture.CreateMany<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()).Returns(_consentAssignedOfferSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _fixture.Inject(_portalRepositories);
    }

    #endregion
}

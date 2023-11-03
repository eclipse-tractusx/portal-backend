/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class SubscriptionConfigurationBusinessLogicTests
{
    private static readonly Guid ExistingCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid NoServiceProviderCompanyId = Guid.NewGuid();
    private readonly IIdentityData _identity;

    private readonly ICompanyRepository _companyRepository;
    private readonly ICollection<ProviderCompanyDetail> _serviceProviderDetails;

    private static readonly Guid OfferSubscriptionId = Guid.NewGuid();
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly ISubscriptionConfigurationBusinessLogic _sut;
    private readonly IIdentityService _identityService;

    public SubscriptionConfigurationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionProcessService = A.Fake<IOfferSubscriptionProcessService>();

        _serviceProviderDetails = new HashSet<ProviderCompanyDetail>();

        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(ExistingCompanyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionsRepository);

        _sut = new SubscriptionConfigurationBusinessLogic(_offerSubscriptionProcessService, _portalRepositories, _identityService);
    }

    #region GetProcessStepsForSubscription

    [Fact]
    public async Task GetProcessStepsForSubscription_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var list = _fixture.CreateMany<ProcessStepData>(5);
        A.CallTo(() => _offerSubscriptionsRepository.GetProcessStepsForSubscription(OfferSubscriptionId))
            .Returns(list.ToAsyncEnumerable());

        // Act
        var result = await _sut.GetProcessStepsForSubscription(OfferSubscriptionId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    #endregion

    #region GetProcessStepData

    [Fact]
    public async Task RetriggerProvider_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, ProcessStepTypeId.RETRIGGER_PROVIDER, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER, null, true))
            .Returns(new ManualProcessStepData(ProcessStepTypeId.RETRIGGER_PROVIDER, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerProvider(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.TRIGGER_PROVIDER })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateClient_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, null, true))
            .Returns(new ManualProcessStepData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerCreateClient(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateTechnicalUser_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, null, true))
            .Returns(new ManualProcessStepData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerCreateTechnicalUser(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerProviderCallback_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, null, false))
            .Returns(new ManualProcessStepData(ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerProviderCallback(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Set ProviderCompanyDetails

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_EmptyProviderDetailsId_ReturnsExpectedResult()
    {
        // Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", "https://www.test.com");
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((Guid.Empty, null!));

        // Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<string>._, A<Action<ProviderCompanyDetail>>._)).MustHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(A<Guid>._, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        _serviceProviderDetails.Should().ContainSingle();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithServiceProviderDetailsId_ReturnsExpectedResult()
    {
        //Arrange
        SetupProviderCompanyDetails();
        const string changedUrl = "https://www.service-url.com";
        var detailsId = Guid.NewGuid();
        var existingUrl = _fixture.Create<string>();
        var providerDetailData = new ProviderDetailData(changedUrl, null);

        ProviderCompanyDetail? initialDetail = null;
        ProviderCompanyDetail? modifyDetail = null;

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((detailsId, existingUrl));

        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(A<Guid>._, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._))
            .Invokes((Guid id, Action<ProviderCompanyDetail> initialize, Action<ProviderCompanyDetail> modifiy) =>
            {
                initialDetail = new ProviderCompanyDetail(id, Guid.Empty, null!, default);
                modifyDetail = new ProviderCompanyDetail(id, Guid.Empty, null!, default);
                initialize(initialDetail);
                modifiy(modifyDetail);
            });

        //Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<string>._, null)).MustNotHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(detailsId, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened(1, Times.OrMore);
        initialDetail.Should().NotBeNull();
        initialDetail!.AutoSetupUrl.Should().Be(existingUrl);
        modifyDetail.Should().NotBeNull();
        modifyDetail!.AutoSetupUrl.Should().Be(changedUrl);
    }

    [Fact]
    public async Task SetServiceProviderCompanyDetailsAsync_WithUnknownUser_ThrowsException()
    {
        //Arrange
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData).ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetServiceProviderCompanyDetailsAsync_WithNotServiceProvider_ThrowsException()
    {
        //Arrange
        A.CallTo(() => _identity.CompanyId).Returns(NoServiceProviderCompanyId);

        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"Company {NoServiceProviderCompanyId} is not an app- or service-provider");
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("http://www.service-url.com")]
    [InlineData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com")]
    public async Task SetServiceProviderCompanyDetailsAsync_WithInvalidUrl_ThrowsException(string? url)
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData(url!, null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    #endregion

    #region Get ProviderCompanyDetails

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithValidUser_ReturnsDetails()
    {
        //Arrange
        SetupProviderCompanyDetails();

        //Act
        var result = await _sut.GetProviderCompanyDetailsAsync().ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithInvalidUser_ThrowsException()
    {
        //Arrange
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        SetupProviderCompanyDetails();

        //Act
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync().ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
    }

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithInvalidServiceProvider_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, ExistingCompanyId))
            .ReturnsLazily(() => (new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de"), false));

        //Act
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync().ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ForbiddenException>(Action);
    }

    #endregion

    #region Setup

    private void SetupProviderCompanyDetails()
    {
        A.CallTo(() => _companyRepository.IsValidCompanyRoleOwner(A<Guid>.That.Matches(x => x == ExistingCompanyId), A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, true));
        A.CallTo(() => _companyRepository.IsValidCompanyRoleOwner(A<Guid>.That.Matches(x => x == NoServiceProviderCompanyId), A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, false));
        A.CallTo(() => _companyRepository.IsValidCompanyRoleOwner(A<Guid>.That.Not.Matches(x => x == ExistingCompanyId || x == NoServiceProviderCompanyId), A<IEnumerable<CompanyRoleId>>._))
            .Returns(((bool, bool))default);

        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<string>._, A<Action<ProviderCompanyDetail>?>._))
            .Invokes((Guid companyId, string dataUrl, Action<ProviderCompanyDetail>? setOptionalParameter) =>
            {
                var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
                setOptionalParameter?.Invoke(providerCompanyDetail);
                _serviceProviderDetails.Add(providerCompanyDetail);
            });

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<Guid>.That.Matches(x => x == ExistingCompanyId)))
            .ReturnsLazily(() => (new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de"), true));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<Guid>.That.Not.Matches(x => x == ExistingCompanyId)))
            .ReturnsLazily(() => ((ProviderDetailReturnData, bool))default);

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<Guid>.That.Matches(x => x == ExistingCompanyId)))
            .ReturnsLazily(() => (Guid.NewGuid(), _fixture.Create<string>()));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<Guid>.That.Not.Matches(x => x == ExistingCompanyId)))
            .Returns((Guid.Empty, null!));
    }

    #endregion
}

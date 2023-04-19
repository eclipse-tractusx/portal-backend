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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class SubscriptionConfigurationBusinessLogicTests
{
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private static readonly string NoServiceProviderIamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8B").ToString();
    private static readonly Guid ExistingCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d489e");

    private readonly ICompanyRepository _companyRepository;
    private readonly ICollection<ProviderCompanyDetail> _serviceProviderDetails;

    private static readonly Guid OfferSubscriptionId = Guid.NewGuid();
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly ISubscriptionConfigurationBusinessLogic _sut;

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

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionsRepository);

        _sut = new SubscriptionConfigurationBusinessLogic(_offerSubscriptionProcessService, _portalRepositories);
    }

    #region GetProcessStepData

    [Fact]
    public async Task GetProcessStepData_WithValidInput_ReturnsExpected()
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

    [Theory]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER, ProcessStepTypeId.TRIGGER_PROVIDER)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION)]
    [InlineData(ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK, false)]
    public async Task TriggerProcessStep_WithValidInput_ReturnsExpected(ProcessStepTypeId retriggerStep, ProcessStepTypeId nextStep, bool mustBePending = true)
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep(processStepId, retriggerStep, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, retriggerStep, null, mustBePending))
            .ReturnsLazily(() => new IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData(OfferSubscriptionId, _fixture.Create<Process>(), processStepId, Enumerable.Repeat(processStep, 1)));

        // Act
        await _sut.TriggerProcessStep(OfferSubscriptionId, retriggerStep, mustBePending);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == nextStep)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task TriggerProcessStep_WithInvalidStep_ThrowsConflictException()
    {
        // Act
        async Task Act() => await _sut.TriggerProcessStep(OfferSubscriptionId, ProcessStepTypeId.START_AUTOSETUP, false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Step {ProcessStepTypeId.START_AUTOSETUP} is not retriggerable");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region Set ProviderCompanyDetails

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_EmptyProviderDetailsId_ReturnsExpectedResult()
    {
        // Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", "https://www.test.com");
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(IamUserId))
            .Returns((Guid.Empty, null!));

        // Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData, IamUserId).ConfigureAwait(false);

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

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(IamUserId))
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
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData, IamUserId).ConfigureAwait(false);

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
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData, Guid.NewGuid().ToString()).ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetServiceProviderCompanyDetailsAsync_WithNotServiceProvider_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData, NoServiceProviderIamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be($"users {NoServiceProviderIamUserId} company is not a service-provider");
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetServiceProviderCompanyDetailsAsync_WithHttpUrl_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("http://www.service-url.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithEmptyUrl_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData(string.Empty, null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData, IamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithToLongUrl_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com", null);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData, IamUserId).ConfigureAwait(false);

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
        var result = await _sut.GetProviderCompanyDetailsAsync(IamUserId).ConfigureAwait(false);

        //Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithInvalidUser_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();

        //Act
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync(Guid.NewGuid().ToString()).ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
    }

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithInvalidServiceProvider_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(CompanyRoleId.SERVICE_PROVIDER, IamUserId))
            .ReturnsLazily(() => (new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de"), false));

        //Act
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync(IamUserId).ConfigureAwait(false);

        //Assert
        await Assert.ThrowsAsync<ForbiddenException>(Action);
    }

    #endregion

    #region Setup

    private void SetupProviderCompanyDetails()
    {
        A.CallTo(() => _companyRepository.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(A<string>.That.Matches(x => x == IamUserId), A<IEnumerable<CompanyRoleId>>._))
            .Returns((ExistingCompanyId, true));
        A.CallTo(() => _companyRepository.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(A<string>.That.Matches(x => x == NoServiceProviderIamUserId), A<IEnumerable<CompanyRoleId>>._))
            .Returns(new ValueTuple<Guid, bool>(Guid.NewGuid(), false));
        A.CallTo(() => _companyRepository.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(A<string>.That.Not.Matches(x => x == IamUserId || x == NoServiceProviderIamUserId), A<IEnumerable<CompanyRoleId>>._))
            .Returns(new ValueTuple<Guid, bool>());

        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<string>._, A<Action<ProviderCompanyDetail>?>._))
            .Invokes((Guid companyId, string dataUrl, Action<ProviderCompanyDetail>? setOptionalParameter) =>
            {
                var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
                setOptionalParameter?.Invoke(providerCompanyDetail);
                _serviceProviderDetails.Add(providerCompanyDetail);
            });

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => (new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de"), true));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<CompanyRoleId>.That.Matches(x => x == CompanyRoleId.SERVICE_PROVIDER), A<string>.That.Not.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => ((ProviderDetailReturnData, bool))default);

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<string>.That.Matches(x => x == IamUserId)))
            .ReturnsLazily(() => (Guid.NewGuid(), _fixture.Create<string>()));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<string>.That.Not.Matches(x => x == IamUserId)))
            .Returns((Guid.Empty, null!));
    }

    #endregion
}

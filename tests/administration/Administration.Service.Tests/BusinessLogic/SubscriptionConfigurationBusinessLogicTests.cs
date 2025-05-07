/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class SubscriptionConfigurationBusinessLogicTests
{
    private static readonly Guid ExistingCompanyId = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid NoServiceProviderCompanyId = Guid.NewGuid();
    private readonly IIdentityData _identity;

    private readonly ICompanyRepository _companyRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly ICollection<ProviderCompanyDetail> _serviceProviderDetails;

    private static readonly Guid OfferSubscriptionId = Guid.NewGuid();
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly ISubscriptionConfigurationBusinessLogic _sut;
    private readonly SubscriptionConfigurationSettings _options;

    public SubscriptionConfigurationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _processStepRepository = A.Fake<IPortalProcessStepRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerSubscriptionProcessService = A.Fake<IOfferSubscriptionProcessService>();

        _serviceProviderDetails = new HashSet<ProviderCompanyDetail>();

        _identity = A.Fake<IIdentityData>();
        var identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(ExistingCompanyId);
        A.CallTo(() => identityService.IdentityData).Returns(_identity);

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>())
            .Returns(_offerSubscriptionsRepository);
        _options = new SubscriptionConfigurationSettings
        {
            EncryptionConfigs =
                   [
                new() { Index = 0, EncryptionKey = Convert.ToHexString(_fixture.CreateMany<byte>(32).ToArray()), CipherMode = CipherMode.CFB, PaddingMode = PaddingMode.PKCS7 },
                       new() { Index = 1, EncryptionKey = Convert.ToHexString(_fixture.CreateMany<byte>(32).ToArray()), CipherMode = CipherMode.CBC, PaddingMode = PaddingMode.PKCS7 },
                   ],
            EncryptionConfigIndex = 1
        };
        _sut = new SubscriptionConfigurationBusinessLogic(_offerSubscriptionProcessService, _portalRepositories, identityService, Options.Create(_options));
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
        var result = await _sut.GetProcessStepsForSubscription(OfferSubscriptionId).ToListAsync();

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
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(processStepId, ProcessStepTypeId.RETRIGGER_PROVIDER, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER, null, true))
            .Returns(new ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>(ProcessStepTypeId.RETRIGGER_PROVIDER, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerProvider(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.TRIGGER_PROVIDER })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateClient_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(processStepId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, null, true))
            .Returns(new ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerCreateClient(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateTechnicalUser_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(processStepId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, null, true))
            .Returns(new ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>(ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerCreateTechnicalUser(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>._,
                A<IEnumerable<ProcessStepTypeId>>.That.IsSameSequenceAs(new[] { ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerProviderCallback_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var processStepId = Guid.NewGuid();
        var processId = Guid.NewGuid();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(processStepId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, ProcessStepStatusId.TODO, processId, DateTimeOffset.Now);
        A.CallTo(() => _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(OfferSubscriptionId, ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, null, false))
            .Returns(new ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>(ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK, _fixture.Create<Process>(), new[] { processStep }, _portalRepositories));

        // Act
        await _sut.RetriggerProviderCallback(OfferSubscriptionId);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData<ProcessTypeId, ProcessStepTypeId>>._,
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
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", "https://example.org/callback", "https://auth.url", "test", "Sup3rS3cureTest!");
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((Guid.Empty, null!));

        // Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        // Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<ProviderDetailsCreationData>._, A<Action<ProviderCompanyDetail>>._)).MustHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(A<Guid>._, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        _serviceProviderDetails.Should().ContainSingle();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithNotExistingAndUrlNull_DoesNothing()
    {
        // Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData(string.Empty, "https://www.test.com", "https://auth.url", "client-id", "Sup3rS3cureTest!");
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((Guid.Empty, null!));

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        // Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<ProviderDetailsCreationData>._, A<Action<ProviderCompanyDetail>>._)).MustNotHaveHappened();
        A.CallTo(() => _companyRepository.RemoveProviderCompanyDetails(A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(A<Guid>._, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithClientSecretNull_ThrowException()
    {
        // Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", "https://www.test.com", "https://auth.url", "test", string.Empty);
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((Guid.Empty, null!));

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_SECRET_MUST_SET.ToString());
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithCallbackUrlChanged_HandlesProcessSteps()
    {
        // Arrange
        SetupProviderCompanyDetails();
        var processId = Guid.NewGuid();
        var processStepId = Guid.NewGuid();
        var providerDetailData = new ProviderDetailData("https://example.org", null, "https://auth.url", "client-id", "Sup3rS3cureTest!");
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((ExistingCompanyId, new ProviderDetails("https://example.org", "https://example.org/callback", "https://auth.url", "test", _fixture.CreateMany<byte>(32).ToArray(), null, 0)));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionRetriggerProcessesForCompanyId(ExistingCompanyId))
            .Returns(new (Process, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>)[]
            {
                (new(processId, ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid()), new(processStepId, ProcessStepTypeId.RETRIGGER_PROVIDER, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow))
            }.ToAsyncEnumerable());

        // Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        // Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<ProviderDetailsCreationData>._, A<Action<ProviderCompanyDetail>>._)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)>>.That.Matches(x => x.Count() == 1 && x.Single().ProcessStepId == processStepId))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>.That.Matches(x => x.Count() == 1 && x.Single().ProcessStepTypeId == ProcessStepTypeId.AWAIT_START_AUTOSETUP))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task SetProviderCompanyDetailsAsync_WithServiceProviderDetailsId_ReturnsExpectedResult()
    {
        //Arrange
        SetupProviderCompanyDetails();
        const string changedUrl = "https://www.service-url.com";
        var detailsId = Guid.NewGuid();
        var existingUrl = _fixture.Create<string>();
        var providerDetailData = new ProviderDetailData(changedUrl, null, "https://auth.url", "test", "Sup3rS3cureTest!");

        ProviderCompanyDetail? initialDetail = null;
        ProviderCompanyDetail? modifyDetail = null;

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((detailsId, new ProviderDetails(existingUrl, string.Empty, "https://auth.url", "test", _fixture.CreateMany<byte>(32).ToArray(), null, 0)));

        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(A<Guid>._, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._))
            .Invokes((Guid id, Action<ProviderCompanyDetail> initialize, Action<ProviderCompanyDetail> modifiy) =>
            {
                initialDetail = new ProviderCompanyDetail(id, Guid.Empty, null!, null!, null!, null!, default);
                modifyDetail = new ProviderCompanyDetail(id, Guid.Empty, null!, null!, null!, null!, default);
                initialize(initialDetail);
                modifiy(modifyDetail);
            });

        //Act
        await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<ProviderDetailsCreationData>._, null)).MustNotHaveHappened();
        A.CallTo(() => _companyRepository.AttachAndModifyProviderCompanyDetails(detailsId, A<Action<ProviderCompanyDetail>>._, A<Action<ProviderCompanyDetail>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
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
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null, "https://auth.url", "test", "Sup3rS3cureTest!");

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetServiceProviderCompanyDetailsAsync_WithNotServiceProvider_ThrowsException()
    {
        //Arrange
        A.CallTo(() => _identity.CompanyId).Returns(NoServiceProviderCompanyId);

        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData("https://www.service-url.com", null, "https://auth.url", "test", "Sup3rS3cureTest!");

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        ex.Message.Should().Be(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_PROVIDER.ToString());
        _serviceProviderDetails.Should().BeEmpty();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("")]
    [InlineData("http://www.service-url.com")]
    public async Task SetServiceProviderCompanyDetailsAsync_WithInvalidUrl_ThrowsException(string url)
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData(url, null, string.Empty, string.Empty, string.Empty);

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Url");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    [Theory]
    [InlineData("https://www.super-duper-long-url-which-is-actually-to-long-to-be-valid-but-it-is-not-long-enough-yet-so-add-a-few-words.com")]
    public async Task SetServiceProviderCompanyDetailsAsync_WithInvalidUrlLengthGreaterThanLimit_ThrowsException(string url)
    {
        //Arrange
        SetupProviderCompanyDetails();
        var providerDetailData = new ProviderDetailData(url, null, "https://auth.url", "test", "Sup3rS3cureTest!");

        //Act
        async Task Action() => await _sut.SetProviderCompanyDetailsAsync(providerDetailData);

        //Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR.ToString());
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        _serviceProviderDetails.Should().BeEmpty();
    }

    #endregion

    #region Delete Auto setup configuration

    [Fact]
    public async Task DeleteOfferProviderCompanyDetailsAsync_ReturnsExpectedResult()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var detailsId = Guid.NewGuid();
        var existingUrl = _fixture.Create<string>();

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((detailsId, new ProviderDetails(existingUrl, string.Empty, "https://auth.url", "test", _fixture.CreateMany<byte>(32).ToArray(), null, 0)));
        //Act
        await _sut.DeleteOfferProviderCompanyDetailsAsync();

        //Assert
        A.CallTo(() => _companyRepository.RemoveProviderCompanyDetails(detailsId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteOfferProviderCompanyDetailsAsync_No_Configuration_Exist()
    {
        //Arrange
        SetupProviderCompanyDetails();
        var detailsId = Guid.NewGuid();

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(ExistingCompanyId))
            .Returns((default, null!));
        //Act
        async Task Action() => await _sut.DeleteOfferProviderCompanyDetailsAsync();

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be(AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_AUTO_SETUP_NOT_FOUND.ToString());
        A.CallTo(() => _companyRepository.RemoveProviderCompanyDetails(detailsId)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region Get ProviderCompanyDetails

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithValidUser_ReturnsDetails()
    {
        //Arrange
        SetupProviderCompanyDetails();

        //Act
        var result = await _sut.GetProviderCompanyDetailsAsync();

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
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync();

        //Assert
        await Assert.ThrowsAsync<ConflictException>(Action);
    }

    [Fact]
    public async Task GetProviderCompanyDetailsAsync_WithInvalidServiceProvider_ThrowsException()
    {
        //Arrange
        SetupProviderCompanyDetails();
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<IEnumerable<CompanyRoleId>>._, ExistingCompanyId))
            .Returns((new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de", "https://auth.url", "client-id", "Sup3rS3cureTest!"), false));

        //Act
        async Task Action() => await _sut.GetProviderCompanyDetailsAsync();

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
            .Returns<(bool, bool)>(default);

        A.CallTo(() => _companyRepository.CreateProviderCompanyDetail(A<Guid>._, A<ProviderDetailsCreationData>._, A<Action<ProviderCompanyDetail>?>._))
            .Invokes((Guid companyId, ProviderDetailsCreationData providerDetailsCreationData, Action<ProviderCompanyDetail>? setOptionalParameter) =>
            {
                var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, providerDetailsCreationData.AutoSetupUrl, providerDetailsCreationData.AuthUrl, providerDetailsCreationData.ClientId, providerDetailsCreationData.ClientSecret, providerDetailsCreationData.EncryptionMode);
                setOptionalParameter?.Invoke(providerCompanyDetail);
                _serviceProviderDetails.Add(providerCompanyDetail);
            });

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<IEnumerable<CompanyRoleId>>.That.Matches(x => x.Contains(CompanyRoleId.SERVICE_PROVIDER) || x.Contains(CompanyRoleId.APP_PROVIDER)), A<Guid>.That.Matches(x => x == ExistingCompanyId)))
            .Returns((new ProviderDetailReturnData(Guid.NewGuid(), Guid.NewGuid(), "https://new-test-service.de", "https://auth.url", "client-id", "Sup3rS3cureTest!"), true));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailAsync(A<IEnumerable<CompanyRoleId>>.That.Matches(x => x.Contains(CompanyRoleId.SERVICE_PROVIDER) || x.Contains(CompanyRoleId.APP_PROVIDER)), A<Guid>.That.Not.Matches(x => x == ExistingCompanyId)))
            .Returns<(ProviderDetailReturnData, bool)>(default);

        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<Guid>.That.Matches(x => x == ExistingCompanyId)))
            .Returns((Guid.NewGuid(), _fixture.Create<ProviderDetails>()));
        A.CallTo(() => _companyRepository.GetProviderCompanyDetailsExistsForUser(A<Guid>.That.Not.Matches(x => x == ExistingCompanyId)))
            .Returns((Guid.Empty, null!));
    }

    #endregion
}

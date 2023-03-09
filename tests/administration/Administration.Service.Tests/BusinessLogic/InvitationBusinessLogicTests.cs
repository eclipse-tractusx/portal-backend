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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class InvitationBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMailingService _mailingService;
    private readonly IOptions<InvitationSettings> _options;
    private readonly string _iamUserId;
    private readonly string _companyName;
    private readonly string _idpName;
    private readonly Guid _companyId;
    private readonly Guid _identityProviderId;
    private readonly Guid _applicationId;
    private readonly Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Exception _error;

    public InvitationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _provisioningManager = A.Fake<IProvisioningManager>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();
        _mailingService = A.Fake<IMailingService>();
        _options = A.Fake<IOptions<InvitationSettings>>();

        _iamUserId = _fixture.Create<string>();
        _companyName = _fixture.Create<string>();
        _idpName = _fixture.Create<string>();
        _companyId = _fixture.Create<Guid>();
        _identityProviderId = _fixture.Create<Guid>();
        _applicationId = _fixture.Create<Guid>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        _error = _fixture.Create<TestException>();
    }

    #region ExecuteInvitation

    [Fact]
    public async Task TestExecuteInvitationSuccess()
    {
        SetupFakes(true);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.organisationName, _companyName)
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, _fixture.CreateEmail())
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        await sut.ExecuteInvitation(invitationData,_iamUserId).ConfigureAwait(false);

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustHaveHappened();
        A.CallTo(() => _provisioningManager.SetupSharedIdpAsync(A<string>.That.IsEqualTo(_idpName), A<string>.That.IsEqualTo(invitationData.organisationName), A<string?>._)).MustHaveHappened();

        A.CallTo(() => _companyRepository.CreateCompany(A<string>.That.IsEqualTo(invitationData.organisationName))).MustHaveHappened();
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>.That.IsEqualTo(IdentityProviderCategoryId.KEYCLOAK_SHARED))).MustHaveHappened();
        A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(A<IdentityProvider>.That.Matches(i => i.Companies.Single().Id == _companyId), A<string>.That.IsEqualTo(_idpName))).MustHaveHappened();
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(A<Guid>.That.IsEqualTo(_companyId),A<CompanyApplicationStatusId>.That.IsEqualTo(CompanyApplicationStatusId.CREATED))).MustHaveHappened();

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
            A<CompanyNameIdpAliasData>.That.Matches(d => d.CompanyId == _companyId),
            A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,
            A<CancellationToken>._)).MustHaveHappened();
        
        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(u => u.FirstName == invitationData.firstName))).MustHaveHappened();
        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Not.Matches(u => u.FirstName == invitationData.firstName))).MustNotHaveHappened();

        A.CallTo(() => _applicationRepository.CreateInvitation(A<Guid>.That.IsEqualTo(_applicationId),A<Guid>._)).MustHaveHappened();

        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>.That.IsEqualTo(invitationData.email),A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationNoEmailThrows()
    {
        SetupFakes(true);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, "")
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData,_iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("email must not be empty (Parameter 'email')");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationNoOrganisationNameThrows()
    {
        SetupFakes(true);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.organisationName, "")
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, _fixture.CreateEmail())
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData,_iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("organisationName must not be empty (Parameter 'organisationName')");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationIamUserNotFoundThrows()
    {
        SetupFakes(true);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.organisationName, _companyName)
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, _fixture.CreateEmail())
            .Create();

        var invalidUserId = _fixture.Create<string>();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData, invalidUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"iamUserId {invalidUserId} is not associated with a companyUser");

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationCreateUserErrorThrows()
    {
        SetupFakes(true);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, _error)
                .Create());

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.organisationName, _companyName)
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, _fixture.CreateEmail())
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestExecuteInvitationCreateUserThrowsThrows()
    {
        SetupFakes(true);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).Throws(_error);

        var invitationData = _fixture.Build<CompanyInvitationData>()
            .With(x => x.organisationName, _companyName)
            .With(x => x.firstName, _fixture.CreateName())
            .With(x => x.lastName, _fixture.CreateName())
            .With(x => x.email, _fixture.CreateEmail())
            .Create();

        var sut = new InvitationBusinessLogic(
            _provisioningManager,
            _userProvisioningService,
            _portalRepositories,
            _mailingService,
            _options);

        Task Act() => sut.ExecuteInvitation(invitationData, _iamUserId);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);
        error.Message.Should().Be(_error.Message);

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).MustHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustNotHaveHappened();
    }

    #endregion

    #region Setup

    private void SetupFakes(bool IsBulkUserCreation)
    {
        A.CallTo(() => _options.Value).Returns(_fixture.Build<InvitationSettings>()
            .With(x => x.InvitedUserInitialRoles, new Dictionary<string,IEnumerable<string>>
            {
                { _fixture.Create<string>(), _fixture.CreateMany<string>() }
            })
            .Create());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);

        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.Empty);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>.That.IsEqualTo(_iamUserId))).Returns(_fixture.Create<Guid>());

        A.CallTo(() => _companyRepository.CreateCompany(A<string>._)).ReturnsLazily((string organisationName) =>
            new Company(_companyId, organisationName, CompanyStatusId.PENDING, _fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._)).ReturnsLazily((IdentityProviderCategoryId categoryId) =>
            new IdentityProvider(_identityProviderId,categoryId,_fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _applicationRepository.CreateCompanyApplication(A<Guid>._, A<CompanyApplicationStatusId>._)).ReturnsLazily((Guid companyId, CompanyApplicationStatusId applicationStatusId) =>
            new CompanyApplication(_applicationId,companyId,applicationStatusId,_fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _provisioningManager.GetNextCentralIdentityProviderNameAsync()).Returns(_idpName);

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }


    #endregion

    [Serializable]
    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        protected TestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Service.Tests;

public class RoleBaseMailServiceTests
{
    private const string BasePortalUrl = "http//base-url.com";
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _idpId = Guid.NewGuid();

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMailingInformationRepository _mailingInformationRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IEnumerable<Guid> _userRoleIds;
    private readonly RoleBaseMailService _sut;

    public RoleBaseMailServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _mailingInformationRepository = A.Fake<IMailingInformationRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRoleIds = _fixture.CreateMany<Guid>(2);

        SetupRepositories();

        _sut = new RoleBaseMailService(_portalRepositories);
    }

    #region RoleBaseSendMailForCompany

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RoleBaseSendMailForCompany_WithUserNameParameter_ReturnsExpectedCalls(bool hasUserNameParameter)
    {
        // Arrange
        var template = new[] { "test-request" };
        var offerName = _fixture.Create<string>();
        var mailParams = new[]
        {
            ("offerName", offerName),
            ("url", BasePortalUrl)
        };
        var userNameParam = hasUserNameParameter
            ? ("offerProviderName", "user")
            : default((string, string)?);
        var receiverRoles = new[]
        {
            new UserRoleConfig("ClientId", new[] { "TestApp Manager", "TestSale Manager" })
        };
        var companyUserData = new (string, string?, string?)[]
        {
            ("TestApp@bmw", "AppFirst", "AppLast"),
            ("TestSale@bmw", "SaleFirst", "SaleLast")
        };

        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(_userRoleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>._, A<Guid>._))
            .Returns(companyUserData.ToAsyncEnumerable());

        // Act
        await _sut.RoleBaseSendMailForCompany(receiverRoles, mailParams, userNameParam, template, _companyId);

        // Assert
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.Any(y => y.ClientId == "ClientId")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>.That.IsSameSequenceAs(_userRoleIds), _companyId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.MAILING)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, A<Guid>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(
            A<Guid>._,
            "TestApp@bmw",
            template.Single(),
            A<IReadOnlyDictionary<string, string>>.That.Matches(x => x.Count == (hasUserNameParameter ? 3 : 2) && x["offerName"] == offerName && x["url"] == BasePortalUrl && (!hasUserNameParameter || x["offerProviderName"] == "AppFirst AppLast")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(
            A<Guid>._,
            "TestSale@bmw",
            template.Single(),
            A<IReadOnlyDictionary<string, string>>.That.Matches(x => x.Count == (hasUserNameParameter ? 3 : 2) && x["offerName"] == offerName && x["url"] == BasePortalUrl && (!hasUserNameParameter || x["offerProviderName"] == "SaleFirst SaleLast")))
            ).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RoleBaseSendMailForCompany_ThrowsConfigurationException()
    {
        // Arrange
        var template = new List<string> { "test-request" };
        var mailParams = new[]
        {
            ("offerName", _fixture.Create<string>()),
            ("url", BasePortalUrl),
        };
        var userNameParam = ("offerProviderName", "user");

        var roleData = _fixture.CreateMany<Guid>(1);
        var receiverRoles = new[]{
            new UserRoleConfig("ClientId", new [] { "App Manager", "Sales Manager" })
        };

        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(roleData.ToAsyncEnumerable());

        // Act
        async Task Action() => await _sut.RoleBaseSendMailForCompany(receiverRoles, mailParams, userNameParam, template, _companyId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Action);
        ex.Message.Should().Be(
            $"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(receiverRoles))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>.That.IsSameSequenceAs(roleData), _companyId)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.MAILING)).MustNotHaveHappened();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<IReadOnlyDictionary<string, string>>._)).MustNotHaveHappened();
    }

    #endregion

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RoleBaseSendMailForIdp_WithUserNameParameter_ReturnsExpectedCalls(bool hasUserNameParameter)
    {
        // Arrange
        var template = "test-request";
        var idpAlias = _fixture.Create<string>();
        var ownerCompanyName = _fixture.Create<string>();
        var mailParams = new[]
        {
            ("idpAlias", idpAlias),
            ("ownerCompanyName", ownerCompanyName)
        };
        var userNameParam = hasUserNameParameter
            ? ("username", "user")
            : default((string, string)?);
        var receiverRoles = new[]
        {
            new UserRoleConfig("ClientId", new[] { "IT Admin", "Company Admin" })
        };
        var companyUserData = new[]
        {
            ("TestIt@bmw", "ItFirst", "ItLast"),
            ("TestCompany@bmw", "CompanyFirst", "CompanyLast")
        };

        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(_userRoleIds.ToAsyncEnumerable());
        A.CallTo(() => _identityProviderRepository.GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(A<IEnumerable<Guid>>._, _idpId))
            .Returns(companyUserData.ToAsyncEnumerable());

        // Act
        await _sut.RoleBaseSendMailForIdp(receiverRoles, mailParams, userNameParam, Enumerable.Repeat(template, 1), _idpId);

        // Assert
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.Any(y => y.ClientId == "ClientId")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(A<IEnumerable<Guid>>.That.IsSameSequenceAs(_userRoleIds), _idpId)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(
            A<Guid>._,
            "TestIt@bmw",
            template,
            A<IReadOnlyDictionary<string, string>>.That.Matches(x => x.Count() == (hasUserNameParameter ? 3 : 2) && x["idpAlias"] == idpAlias && x["ownerCompanyName"] == ownerCompanyName && (!hasUserNameParameter || x["username"] == "ItFirst ItLast"))
            )).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(
            A<Guid>._,
            "TestCompany@bmw",
            template,
            A<IReadOnlyDictionary<string, string>>.That.Matches(x => x.Count() == (hasUserNameParameter ? 3 : 2) && x["idpAlias"] == idpAlias && x["ownerCompanyName"] == ownerCompanyName && (!hasUserNameParameter || x["username"] == "CompanyFirst CompanyLast"))
            )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RoleBaseSendMailForIdp_ThrowsConfigurationException()
    {
        // Arrange
        var template = new List<string> { "test-request" };
        var mailParams = new[]
        {
            ("idpAlias", _fixture.Create<string>()),
            ("ownerCompanyName", "test")
        };
        var userNameParam = ("username", "user");

        var roleData = _fixture.CreateMany<Guid>(1);
        var receiverRoles = new[]{
            new UserRoleConfig("ClientId", new [] { "IT Admin", "Company Admin" })
        };

        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(roleData.ToAsyncEnumerable());

        // Act
        async Task Action() => await _sut.RoleBaseSendMailForIdp(receiverRoles, mailParams, userNameParam, template, _idpId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Action);
        ex.Message.Should().Be($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(receiverRoles))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(A<IEnumerable<Guid>>.That.IsSameSequenceAs(roleData), _idpId)).MustNotHaveHappened();
        A.CallTo(() => _mailingInformationRepository.CreateMailingInformation(A<Guid>._, A<string>._, A<string>._, A<IReadOnlyDictionary<string, string>>._)).MustNotHaveHappened();
    }

    #region Setup

    private void SetupRepositories()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IMailingInformationRepository>()).Returns(_mailingInformationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);
        _fixture.Inject(_portalRepositories);
    }

    #endregion
}

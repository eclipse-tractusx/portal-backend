/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class UserUploadBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IOptions<UserSettings> _options;
    private readonly IFormFile _document;
    private readonly Guid _identityProviderId;
    private readonly string _iamUserId;
    private readonly string _clientId;
    private readonly UserSettings _settings;
    private readonly Encoding _encoding;
    private readonly Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Exception _error;
    private readonly Random _random;

    public UserUploadBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _random = new Random();

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _options = A.Fake<IOptions<UserSettings>>();

        _document = A.Fake<IFormFile>();
        _identityProviderId = _fixture.Create<Guid>();
        _iamUserId = _fixture.Create<string>();
        _clientId = _fixture.Create<string>();
        _settings = _fixture.Build<UserSettings>().With(x => x.Portal, _fixture.Build<UserSetting>().With(x => x.KeyCloakClientID, _clientId).Create()).Create();
        _encoding = _fixture.Create<Encoding>();

        _processLine = A.Fake<Func<UserCreationRoleDataIdpInfo,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        _error = _fixture.Create<TestException>();
    }

    #region UploadOwnCompanyIdpUsersAsync
    [Fact]
    public async Task TestSetup()
    {
        SetupFakes(new [] { HeaderLine() });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _userProvisioningService.GetCompanyNameIdpAliasData(A<Guid>.That.IsEqualTo(_identityProviderId), A<string>.That.IsEqualTo(_iamUserId))).MustHaveHappened();
        result.Should().NotBeNull();
        result.Created.Should().Be(0);
        result.Error.Should().Be(0);
        result.Total.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TestUserCreationAllSuccess()
    {
        SetupFakes(new [] {
            HeaderLine(),
            NextLine(),
            NextLine(),
            NextLine(),
            NextLine(),
            NextLine()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Created.Should().Be(5);
        result.Error.Should().Be(0);
        result.Total.Should().Be(5);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TestUserCreationHeaderParsingThrows()
    {
        var invalidHeader = _fixture.Create<string>();

        SetupFakes(new [] {
            invalidHeader,
            NextLine(),
            NextLine(),
            NextLine(),
            NextLine(),
            NextLine()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        async Task Act() => await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"invalid format: expected 'FirstName', got '{invalidHeader}' (Parameter 'document')");
    }

    [Fact]
    public async Task TestUserCreationCreationError()
    {
        var creationInfo = _fixture.Create<UserCreationRoleDataIdpInfo>();

        SetupFakes(new [] {
            HeaderLine(),
            NextLine(),
            NextLine(),
            NextLine(creationInfo),
            NextLine(),
            NextLine()
        });

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatches(info, creationInfo))))
            .ReturnsLazily(
                (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                    .With(x => x.CompanyUserId, Guid.Empty)
                    .With(x => x.UserName, creationInfo.UserName)
                    .With(x => x.Error, _error)
                    .Create());

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatches(info, creationInfo)))).MustHaveHappened();

        result.Should().NotBeNull();
        result.Created.Should().Be(4);
        result.Error.Should().Be(1);
        result.Total.Should().Be(5);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: {_error.Message}");
    }

    [Fact]
    public async Task TestUserCreationParsingError()
    {
        SetupFakes(new [] {
            HeaderLine(),
            NextLine(),
            NextLine(),
            _fixture.Create<string>(),
            NextLine(),
            NextLine()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Created.Should().Be(4);
        result.Error.Should().Be(1);
        result.Total.Should().Be(5);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be("line: 3, message: value for LastName type string expected (Parameter 'document')");
    }

    [Fact]
    public async Task TestUserCreationCreationThrows()
    {
        var creationInfo = _fixture.Create<UserCreationRoleDataIdpInfo>();

        SetupFakes(new [] {
            HeaderLine(),
            NextLine(),
            NextLine(),
            NextLine(creationInfo),
            NextLine(),
            NextLine()
        });

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatches(info, creationInfo))))
            .Throws(_error);

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanyIdpUsersAsync(_identityProviderId, _document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatches(info, creationInfo)))).MustHaveHappened();

        result.Should().NotBeNull();
        result.Created.Should().Be(2);
        result.Error.Should().Be(1);
        result.Total.Should().Be(3);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: {_error.Message}");
    }

    #endregion

    #region UploadOwnCompanySharedIdpUsersAsync

    [Fact]
    public async Task TestSetupSharedIdp()
    {
        SetupFakes(new [] { HeaderLineSharedIdp() });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _userProvisioningService.GetCompanyNameSharedIdpAliasData(A<string>.That.IsEqualTo(_iamUserId),A<Guid?>._)).MustHaveHappened();
        result.Should().NotBeNull();
        result.Created.Should().Be(0);
        result.Error.Should().Be(0);
        result.Total.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TestUserCreationSharedIdpAllSuccess()
    {
        SetupFakes(new [] {
            HeaderLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Created.Should().Be(5);
        result.Error.Should().Be(0);
        result.Total.Should().Be(5);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TestUserCreationSharedIdpHeaderParsingThrows()
    {
        var invalidHeader = _fixture.Create<string>();

        SetupFakes(new [] {
            invalidHeader,
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        async Task Act() => await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"invalid format: expected 'FirstName', got '{invalidHeader}' (Parameter 'document')");
    }

    [Fact]
    public async Task TestUserCreationSharedIdpCreationError()
    {
        var creationInfo = _fixture.Create<UserCreationRoleDataIdpInfo>();

        SetupFakes(new [] {
            HeaderLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(creationInfo),
            NextLineSharedIdp(),
            NextLineSharedIdp()
        });

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatchesSharedIdp(info, creationInfo))))
            .ReturnsLazily(
                (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                    .With(x => x.CompanyUserId, Guid.Empty)
                    .With(x => x.UserName, creationInfo.UserName)
                    .With(x => x.Error, _error)
                    .Create());

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatchesSharedIdp(info, creationInfo)))).MustHaveHappened();

        result.Should().NotBeNull();
        result.Created.Should().Be(4);
        result.Error.Should().Be(1);
        result.Total.Should().Be(5);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: {_error.Message}");
    }

    [Fact]
    public async Task TestUserCreationSharedIdpParsingError()
    {
        SetupFakes(new [] {
            HeaderLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            _fixture.Create<string>(),
            NextLineSharedIdp(),
            NextLineSharedIdp()
        });

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Created.Should().Be(4);
        result.Error.Should().Be(1);
        result.Total.Should().Be(5);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be("line: 3, message: value for LastName type string expected (Parameter 'document')");
    }

    [Fact]
    public async Task TestUserCreationSharedIdpCreationThrows()
    {
        var creationInfo = _fixture.Create<UserCreationRoleDataIdpInfo>();

        SetupFakes(new [] {
            HeaderLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(),
            NextLineSharedIdp(creationInfo),
            NextLineSharedIdp(),
            NextLineSharedIdp()
        });

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatchesSharedIdp(info, creationInfo))))
            .Throws(_error);

        var sut = new UserUploadBusinessLogic(_userProvisioningService,_options);

        var result = await sut.UploadOwnCompanySharedIdpUsersAsync(_document, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>.That.Matches(info => CreationInfoMatchesSharedIdp(info, creationInfo)))).MustHaveHappened();

        result.Should().NotBeNull();
        result.Created.Should().Be(2);
        result.Error.Should().Be(1);
        result.Total.Should().Be(3);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: {_error.Message}");
    }

    #endregion

    #region Setup

    private void SetupFakes(IEnumerable<string> lines)
    {
        A.CallTo(() => _document.ContentType).Returns("text/csv");
        A.CallTo(() => _document.OpenReadStream()).ReturnsLazily(() => new AsyncEnumerableStringStream(lines.ToAsyncEnumerable(), _encoding));

        A.CallTo(() =>_options.Value).Returns(_settings);

        A.CallTo(() => _userProvisioningService.GetCompanyNameIdpAliasData(A<Guid>.That.IsEqualTo(_identityProviderId), A<string>.That.IsEqualTo(_iamUserId)))
            .Returns((_fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, false).Create(),_fixture.Create<string>()));

        A.CallTo(() => _userProvisioningService.GetCompanyNameSharedIdpAliasData(A<string>.That.IsEqualTo(_iamUserId),A<Guid?>._))
            .Returns((_fixture.Build<CompanyNameIdpAliasData>().With(x => x.IsSharedIdp, true).Create(),_fixture.Create<string>()));

        A.CallTo(() => _userProvisioningService.GetOwnCompanyPortalRoleDatas(A<string>._,A<IEnumerable<string>>._, A<string>._))
            .ReturnsLazily((string clientId, IEnumerable<string> roles, string _) =>
                roles.Select(role => _fixture.Build<UserRoleData>().With(x => x.ClientClientId, clientId).With(x => x.UserRoleText, role).Create()));

        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _processLine(A<UserCreationRoleDataIdpInfo>._)).ReturnsLazily(
            (UserCreationRoleDataIdpInfo creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, (Exception?)null)
                .Create());
    }

    private static string HeaderLine() =>
        string.Join(",",new [] {
            UserUploadBusinessLogic.CsvHeaders.FirstName,
            UserUploadBusinessLogic.CsvHeaders.LastName,
            UserUploadBusinessLogic.CsvHeaders.Email,
            UserUploadBusinessLogic.CsvHeaders.ProviderUserName,
            UserUploadBusinessLogic.CsvHeaders.ProviderUserId,
            UserUploadBusinessLogic.CsvHeaders.Roles });        

    private string NextLine() =>
        string.Join(",",_fixture.CreateMany<string>(_random.Next(5,10)));        

    private static string HeaderLineSharedIdp() =>
        string.Join(",",new [] {
            UserUploadBusinessLogic.CsvHeaders.FirstName,
            UserUploadBusinessLogic.CsvHeaders.LastName,
            UserUploadBusinessLogic.CsvHeaders.Email,
            UserUploadBusinessLogic.CsvHeaders.Roles });        

    private string NextLineSharedIdp() =>
        string.Join(",",_fixture.CreateMany<string>(_random.Next(3,7)));        

    private static string NextLine(UserCreationRoleDataIdpInfo userCreationInfoIdp) =>
        string.Join(",", new [] {
            userCreationInfoIdp.FirstName,
            userCreationInfoIdp.LastName,
            userCreationInfoIdp.Email,
            userCreationInfoIdp.UserName,
            userCreationInfoIdp.UserId
        }.Concat(userCreationInfoIdp.RoleDatas.Select(d => d.UserRoleText)));

    private static string NextLineSharedIdp(UserCreationRoleDataIdpInfo userCreationInfoIdp) =>
        string.Join(",", new [] {
            userCreationInfoIdp.FirstName,
            userCreationInfoIdp.LastName,
            userCreationInfoIdp.Email,
        }.Concat(userCreationInfoIdp.RoleDatas.Select(d => d.UserRoleText)));

    private static bool CreationInfoMatches(UserCreationRoleDataIdpInfo creationInfo, UserCreationRoleDataIdpInfo other) =>
        creationInfo.FirstName == other.FirstName &&
            creationInfo.LastName == other.LastName &&
            creationInfo.Email == other.Email &&
            creationInfo.RoleDatas.Select(d => d.UserRoleText).SequenceEqual(other.RoleDatas.Select(d => d.UserRoleText)) &&
            creationInfo.UserId == other.UserId &&
            creationInfo.UserName == other.UserName;

    private static bool CreationInfoMatchesSharedIdp(UserCreationRoleDataIdpInfo creationInfo, UserCreationRoleDataIdpInfo other) =>
        creationInfo.FirstName == other.FirstName &&
            creationInfo.LastName == other.LastName &&
            creationInfo.Email == other.Email &&
            creationInfo.RoleDatas.Select(d => d.UserRoleText).SequenceEqual(other.RoleDatas.Select(d => d.UserRoleText));
    
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

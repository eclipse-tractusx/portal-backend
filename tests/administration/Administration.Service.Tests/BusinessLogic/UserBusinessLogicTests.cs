/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic.Tests;

public class UserBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IMailingService _mailingService;
    private readonly IMockLogger<UserBusinessLogic> _mockLogger;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly IOptions<UserSettings> _options;
    private readonly Guid _identityProviderId;
    private readonly string _iamUserId;
    private readonly Func<UserCreationInfoIdp,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> _processLine;
    private readonly Exception _error;
    private readonly Random _random;

    public UserBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _random = new Random();

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _mailingService = A.Fake<IMailingService>();
        _mockLogger = A.Fake<UserBusinessLogicTests.IMockLogger<UserBusinessLogic>>();
        _logger = new UserBusinessLogicTests.MockLogger<UserBusinessLogic>(_mockLogger);
        _options = _fixture.Create<IOptions<UserSettings>>();

        _identityProviderId = _fixture.Create<Guid>();
        _iamUserId = _fixture.Create<string>();

        _processLine = A.Fake<Func<UserCreationInfoIdp,(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>>();

        _error = _fixture.Create<TestException>();
    }

    #region CreateOwnCompanyUsersAsync
    [Fact]
    public async void TestUserCreationAllSuccess()
    {
        SetupFakes();

        var userList = _fixture.Create<IEnumerable<UserCreationInfo>>();        

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _mockLogger.MockLog(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == userList.Count());

        result.Should().NotBeNull();
        result.Should().HaveSameCount(userList);
        result.Should().Match(r => r.SequenceEqual(userList.Select(u => u.eMail)));
    }

    [Fact]
    public async void TestUserCreationCreationError()
    {
        SetupFakes();

        var userCreationInfo = _fixture.Create<UserCreationInfo>();
        var error = _fixture.Create<TestException>();

        var userList = new [] {
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>(),
            userCreationInfo,
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>()
        };

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).ReturnsLazily(
            (UserCreationInfoIdp creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
                .With(x => x.UserName, creationInfo.UserName)
                .With(x => x.Error, error)
                .Create());

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        var result = await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.MockLog(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 4);

        result.Should().NotBeNull();
        result.Should().HaveCount(4);
    }

    [Fact]
    public async void TestUserCreationCreationThrows()
    {
        SetupFakes();

        var userCreationInfo = _fixture.Create<UserCreationInfo>();
        var expected = _fixture.Create<TestException>();

        var userList = new [] {
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>(),
            userCreationInfo,
            _fixture.Create<UserCreationInfo>(),
            _fixture.Create<UserCreationInfo>()
        };

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).Throws(() => expected);

        var sut = new UserBusinessLogic(
            null!,
            _userProvisioningService,
            null!,
            _portalRepositories,
            _mailingService,
            _logger,
            _options);

        async Task Act() => await sut.CreateOwnCompanyUsersAsync(userList, _iamUserId).ToListAsync().ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<TestException>(Act).ConfigureAwait(false);

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>.That.Matches(u =>
            u.FirstName == userCreationInfo.firstName &&
            u.LastName == userCreationInfo.lastName &&
            u.Email == userCreationInfo.eMail &&
            u.Roles == userCreationInfo.Roles            
        ))).MustHaveHappenedOnceExactly();

        A.CallTo(() => _mockLogger.MockLog(A<LogLevel>.That.IsEqualTo(LogLevel.Error), A<Exception?>._)).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._,A<Dictionary<string,string>>._,A<List<string>>._)).MustHaveHappenedANumberOfTimesMatching(times => times == 2);

        error.Should().BeSameAs(expected);
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._,A<string>._,A<IAsyncEnumerable<UserCreationInfoIdp>>._,A<CancellationToken>._))
            .ReturnsLazily((CompanyNameIdpAliasData companyNameIdpAliasData, string keycloakClientId, IAsyncEnumerable<UserCreationInfoIdp> userCreationInfos, CancellationToken cancellationToken) =>
                userCreationInfos.Select(userCreationInfo => _processLine(userCreationInfo)));

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(null!);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._, A<IdentityProviderCategoryId>._))
            .Returns(_fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>()
                .With(x => x.IdpAliase, new [] { _fixture.Create<string>() })
                .Create());

        A.CallTo(() => _processLine(A<UserCreationInfoIdp>._)).ReturnsLazily(
            (UserCreationInfoIdp creationInfo) => _fixture.Build<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)>()
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

    private static string NextLine(UserCreationInfoIdp userCreationInfoIdp) =>
        string.Join(",", new [] {
            userCreationInfoIdp.FirstName,
            userCreationInfoIdp.LastName,
            userCreationInfoIdp.Email,
            userCreationInfoIdp.UserName,
            userCreationInfoIdp.UserId
        }.Concat(userCreationInfoIdp.Roles));

    private static string NextLineSharedIdp(UserCreationInfoIdp userCreationInfoIdp) =>
        string.Join(",", new [] {
            userCreationInfoIdp.FirstName,
            userCreationInfoIdp.LastName,
            userCreationInfoIdp.Email,
        }.Concat(userCreationInfoIdp.Roles));

    private static bool CreationInfoMatches(UserCreationInfoIdp creationInfo, UserCreationInfoIdp other) =>
        creationInfo.FirstName == other.FirstName &&
        creationInfo.LastName == other.LastName &&
        creationInfo.Email == other.Email &&
        creationInfo.Roles.SequenceEqual(other.Roles) &&
        creationInfo.UserId == other.UserId &&
        creationInfo.UserName == other.UserName;

    private static bool CreationInfoMatchesSharedIdp(UserCreationInfoIdp creationInfo, UserCreationInfoIdp other) =>
        creationInfo.FirstName == other.FirstName &&
        creationInfo.LastName == other.LastName &&
        creationInfo.Email == other.Email &&
        creationInfo.Roles.SequenceEqual(other.Roles);

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

    public interface IMockLogger<T>
    {
        void MockLog(LogLevel logLevel, Exception? exception);
    }

    public class MockLogger<T> : ILogger<T>
    {
        private readonly IMockLogger<T> _logger;

        public MockLogger(IMockLogger<T> logger)
        {
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state) => new TestDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState,Exception?,string> formatter) =>
            _logger.MockLog(logLevel,exception);
        
        public class TestDisposable : IDisposable
        {
            public void Dispose() {}
        }
    }
}

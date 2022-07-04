﻿using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.Notification.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.Notification.Service.Tests
{
    public class NotificationBusinessLogicTests
    {
        private readonly IFixture _fixture;
        private readonly IPortalRepositories _portalRepositories;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly CompanyUser _companyUser;

        public NotificationBusinessLogicTests()
        {
            _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
            var (companyUser, _) = CreateTestUserPair();
            _companyUser = companyUser;
            _portalRepositories = A.Fake<IPortalRepositories>();
            _notificationRepository = A.Fake<INotificationRepository>();
            _userRepository = A.Fake<IUserRepository>();
            A.CallTo(() => _userRepository.IsUserWithIdExisting(companyUser.Id))
                .ReturnsLazily(() => Task.FromResult(true));
            A.CallTo(() => _userRepository.IsUserWithIdExisting(A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
                .ReturnsLazily(() => Task.FromResult(false));
        }

        [Fact]
        public async Task CreateNotification_WithInvalidNotificationType_ThrowsArgumentException()
        {
            // Arrange
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
            var sut = _fixture.Create<NotificationBusinessLogic>();

            // Act
            try
            {
                await sut.CreateNotification(new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader", (NotificationTypeId)666, NotificationStatusId.UNREAD, null, null), _companyUser.Id);
            }
            catch (ArgumentException e)
            {
                // Assert
                e.ParamName.Should().Be("notificationTypeId");
                return;
            }

            // Must not reach that code because of the exception
            false.Should().BeTrue();
        }

        [Fact]
        public async Task CreateNotification_WithInvalidNotificationStatus_ThrowsArgumentException()
        {
            // Arrange
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
            var sut = _fixture.Create<NotificationBusinessLogic>();

            // Act
            try
            {
                await sut.CreateNotification(new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader", NotificationTypeId.INFO, (NotificationStatusId)666, null, null), _companyUser.Id);
            }
            catch (ArgumentException e)
            {
                // Assert
                e.ParamName.Should().Be("notificationStatusId");
                return;
            }

            // Must not reach that code because of the exception
            false.Should().BeTrue();
        }

        [Fact]
        public async Task CreateNotification_WithNotExistingCompanyUser_ThrowsArgumentException()
        {
            // Arrange
            A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
            A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
            _fixture.Inject(_portalRepositories);
            var sut = _fixture.Create<NotificationBusinessLogic>();

            // Act
            try
            {
                await sut.CreateNotification(new NotificationCreationData(DateTimeOffset.Now, "That's a title", "Here is a message for the reader", NotificationTypeId.INFO, NotificationStatusId.UNREAD, null, null), Guid.NewGuid());
            }
            catch (ArgumentException e)
            {
                // Assert
                e.ParamName.Should().Be("companyUserId");
                return;
            }

            // Must not reach that code because of the exception
            false.Should().BeTrue();
        }

        private (CompanyUser, IamUser) CreateTestUserPair()
        {
            var companyUser = _fixture.Build<CompanyUser>()
                .Without(u => u.IamUser)
                .Create();
            var iamUser = _fixture.Build<IamUser>()
                .With(u => u.CompanyUser, companyUser)
                .Create();
            companyUser.IamUser = iamUser;
            return (companyUser, iamUser);
        }
    }
}

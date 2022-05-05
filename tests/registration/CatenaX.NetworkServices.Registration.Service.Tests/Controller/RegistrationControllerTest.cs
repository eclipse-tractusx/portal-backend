using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Registration.Service.Controllers;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using Microsoft.Extensions.Logging;
using CatenaX.NetworkServices.Registration.Service.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Registration.Service.Tests
{
    public class RegistrationControllerTest
    {
        private readonly IFixture _fixture;
        private readonly RegistrationController controller;
        private readonly IRegistrationBusinessLogic registrationBusineesLogicFake;
        private readonly ILogger<RegistrationController> registrationLoggerFake;
        public RegistrationControllerTest()
        {
            _fixture = new Fixture();
            registrationBusineesLogicFake = A.Fake<IRegistrationBusinessLogic>();
            registrationLoggerFake = A.Fake<ILogger<RegistrationController>>();
            this.controller = new RegistrationController(registrationLoggerFake, registrationBusineesLogicFake);
        }

        [Fact]
        public async Task Get_WhenThereAreInvitedUsers_ShouldReturnActionResultOfInvitedUsersWith200StatusCode()
        {
            //Arrange
            Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
            A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersDetail(id))
                .Returns(invitedUserMapper);

            //Act
            var result = this.controller.GetInvitedUserDetailAsync(id);
            await foreach (var item in result)
            {
                //Assert
                A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersDetail(id)).MustHaveHappenedOnceExactly();
                Assert.NotNull(item);
                Assert.IsType<InvitedUser>(item);
            }

        }

        [Fact]
        public async Task GetInvitedUsersDetail_WhenIdisNull_ShouldThrowException()
        {
            //Arrange
            Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            var invitedUserMapper = _fixture.CreateMany<InvitedUser>(3).ToAsyncEnumerable();
            A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersDetail(id))
                .Returns(invitedUserMapper);

            //Act
            var result = this.controller.GetInvitedUserDetailAsync(Guid.Empty);

            //Assert
            await foreach (var item in result)
            {
                A.CallTo(() => registrationBusineesLogicFake.GetInvitedUsersDetail(Guid.Empty)).Throws(new Exception());
            }

        }

    }
}
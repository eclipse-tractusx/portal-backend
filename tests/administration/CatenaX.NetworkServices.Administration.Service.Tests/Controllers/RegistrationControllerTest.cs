using System;
using System.Threading.Tasks;
using AutoFixture;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Controllers;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using Xunit;

namespace CatenaX.NetworkServices.Administration.Service.Tests.Controllers
{
    public class RegistrationControllerTest
    {
        private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
        private readonly IRegistrationBusinessLogic _logic;
        private readonly RegistrationController _controller;

        public RegistrationControllerTest()
        {
            _logic = A.Fake<IRegistrationBusinessLogic>();
            this._controller = new RegistrationController(_logic);
            _controller.AddControllerContextWithClaim(IamUserId);
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, id))
                      .Returns(true);

            //Act
            var result = await this._controller.ApprovePartnerRequest(id).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, id)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Test2()
        {
            //Arrange
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, Guid.Empty))
                      .Returns(false);

            //Act
            var result = await this._controller.ApprovePartnerRequest(Guid.Empty).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, Guid.Empty)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.False(result);
        }
    }
}
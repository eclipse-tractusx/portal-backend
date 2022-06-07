using AutoFixture;
using FakeItEasy;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CatenaX.NetworkServices.Administration.Service.Controllers;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using Microsoft.Extensions.Logging;

namespace CatenaX.NetworkServices.Administration.Service.Tests
{
    public class RegistrationControllerTest
    {
        private readonly IRegistrationBusinessLogic _logic;
        private readonly IFixture _fixture;
        private readonly RegistrationController controller;
        public RegistrationControllerTest()
        {
            _fixture = new Fixture();
            _logic = A.Fake<IRegistrationBusinessLogic>();
            this.controller = new RegistrationController(_logic);
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            Guid id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
            A.CallTo(() => _logic.ApprovePartnerRequest(id))
                      .Returns(true);

            //Act
            var result = await this.controller.ApprovePartnerRequest(id).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(id)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Test2()
        {
            //Arrange
            A.CallTo(() => _logic.ApprovePartnerRequest(Guid.Empty))
                      .Returns(false);

            //Act
            var result = await this.controller.ApprovePartnerRequest(Guid.Empty).ConfigureAwait(false);

            //Assert
            A.CallTo(() => _logic.ApprovePartnerRequest(Guid.Empty)).MustHaveHappenedOnceExactly();
            Assert.IsType<bool>(result);
            Assert.False(result);
        }
    }
}
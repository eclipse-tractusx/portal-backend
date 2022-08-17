using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Controllers;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using Xunit;

namespace CatenaX.NetworkServices.Administration.Service.Tests.Controllers
{
    public class PartnerNetworkControllerTest
    {
        private readonly IPartnerNetworkBusinessLogic _logic;
        private readonly PartnerNetworkController _controller;

       // private readonly IAsyncEnumerable<string> companyBpns;

        public PartnerNetworkControllerTest()
        {
            _logic = A.Fake<IPartnerNetworkBusinessLogic>();
            this._controller = new PartnerNetworkController(_logic);
        }

        [Fact]
        public async Task GetAllMemberCompaniesBPN_Test()
        {
            //Arrange
            
            A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync());

            //Act
            var result = this._controller.GetAllMemberCompaniesBPNAsync();

            //Assert
            A.CallTo(() => _logic.GetAllMemberCompaniesBPNAsync()).MustHaveHappenedOnceExactly();
            //Assert.IsType<string>(strin);
            //var companyBpnsList = companyBpns.First();
            //result.Should().Be(IAsyncEnumerable<string>);
        }
    }
}
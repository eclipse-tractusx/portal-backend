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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Keycloak.Net;
using CatenaX.NetworkServices.Keycloak.Factory;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Keycloak.DBAccess;
using Keycloak.Net.Models.Roles;
using CatenaX.NetworkServices.Provisioning.DBAccess;

namespace CatenaX.NetworkServices.Provisioning.Library.Tests
{
    public class ProvisioningManagerTest
    {
        private readonly IFixture _fixture;
        private readonly KeycloakClient _CentralIdp;
        private readonly KeycloakClient _SharedIdp;
        private readonly IKeycloakDBAccess _KeycloakDBAccess;
        private readonly IProvisioningDBAccess _ProvisioningDBAccess;
        private readonly ProvisioningSettings _Settings;
        private readonly IProvisioningManager _provisionManager;
        private readonly ProvisioningManager _provisionManagerConcrete;
        private readonly IKeycloakFactory _keyCloakFactory;
        private readonly IOptions<ProvisioningSettings> _options;
        public ProvisioningManagerTest()
        {
            _fixture = new Fixture();
            _keyCloakFactory = A.Fake<IKeycloakFactory>();
            _KeycloakDBAccess = A.Fake<IKeycloakDBAccess>();
            _options = A.Fake<IOptions<ProvisioningSettings>>();
            _ProvisioningDBAccess = A.Fake<IProvisioningDBAccess>();
            _provisionManager = new ProvisioningManager(_keyCloakFactory, _KeycloakDBAccess, _ProvisioningDBAccess, _options);
        }

        [Fact]
        public async Task Get_WhenThereAreClientId_ShouldReturnClientRoleForUser()
        {
            //Arrange
            //Guid id = new Guid("7eab8e16-8298-4b41-953b-515745423658");
            //string clientId = "catenax-registration";
            //var lstRole = _fixture.CreateMany<Role>(2).AsEnumerable();
            //var centralIdp = A.Fake<KeycloakClient>();
            //A.CallTo(() => _keyCloakFactory.CreateKeycloakClient("central"))
            //.Returns(centralIdp);
            //A.CallTo(() => centralIdp.GetClientRoleMappingsForUserAsync("idp7", "0b5cc673-4036-4ad4-a653-daf91b52e0b6", id.ToString()))
            //.CallsBaseMethod();
            //Act
            //var result = await _provisionManager.GetClientRoleMappingsForUserAsync("0b5cc673-4036-4ad4-a653-daf91b52e0b6", clientId);

            //Assert
            // A.CallTo(() => _provisionManagerConcrete.GetIdOfClientFromClientIDAsync(clientId)).MustHaveHappenedOnceExactly();
            //A.CallTo(() => _CentralIdp.GetClientRoleMappingsForUserAsync("idp7", "0b5cc673-4036-4ad4-a653-daf91b52e0b6", id.ToString())).MustHaveHappenedOnceExactly();
            //Assert.NotNull(result);
        }

    }
}
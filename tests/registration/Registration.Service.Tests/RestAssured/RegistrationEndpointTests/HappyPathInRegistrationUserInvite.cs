using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using PasswordGenerator;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class HappyPathInRegistrationUserInvite
{
    private static RegistrationEndpointHelper _regEndpointHelper;
    
    [Fact]
    public async Task Scenario_HappyPathInRegistrationUSerInvite()
    {
        var now = DateTime.Now;
        var userCompanyName = "Test-Catena-X-" + now.Month + now.Day + now.Hour + now.Minute + now.Second; 
        _regEndpointHelper = new RegistrationEndpointHelper();
        await _regEndpointHelper.ExecuteInvitation(userCompanyName);
        Thread.Sleep(10000);
        _regEndpointHelper.InviteNewUser();
    }
}
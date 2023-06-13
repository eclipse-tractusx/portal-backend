using Xunit;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Registration.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class HappyPathInRegistrationUserInvite
{
    [Fact]
    public async Task Scenario_HappyPathInRegistrationUserInvite()
    {
        var now = DateTime.Now;
        var userCompanyName = $"Test-Catena-X_{now:s}";
        await RegistrationEndpointHelper.ExecuteInvitation(userCompanyName);
        Thread.Sleep(10000);
        RegistrationEndpointHelper.InviteNewUser();
    }
}
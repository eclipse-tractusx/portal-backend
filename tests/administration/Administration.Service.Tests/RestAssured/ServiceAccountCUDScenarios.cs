using System.IdentityModel.Tokens.Jwt;
using Castle.Core.Internal;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public class ServiceAccountCUDScenarios
{
    //Scenario - Create a new service account
    [Fact]
    public async Task Scenario_HappyPathCreateServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();

        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // get a snapshot of current existing service accounts
        var existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        //create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        if (newServiceAccount != null)
        {
            //check if the new service account is added (validation against the previous taken snapshot)
            var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

            if (existingServiceAccounts != null)
            {
                var checkAccountIsNew =
                    existingServiceAccounts.Where(t => t.ServiceAccountId == newServiceAccount.ServiceAccountId);
                Assert.Empty(checkAccountIsNew);
            }

            if (updatedServiceAccounts == null) throw new Exception("List of service accounts was not found");
            var checkAccountAdded =
                updatedServiceAccounts.Where(t => t.ServiceAccountId == newServiceAccount.ServiceAccountId);
            Assert.NotEmpty(checkAccountAdded);

            //fetch the serviceAccount token and validate if the token includes a attribute "bpn"
            var token = RetrieveTechUserToken(newServiceAccount.ClientId, newServiceAccount.Secret);
            if (token.IsNullOrEmpty())
                throw new Exception("Token for new technical user could not be fetched correctly");
            Assert.NotEmpty(token);
            Assert.True(CheckTokenForAttribute(token, "bpn"));
        }
        else throw new Exception("Service Account was not created correctly");
    }

    //Scenario - Create a new service account and update the same
    [Fact]
    public async Task Scenario_HappyPathCreateAndUpdateServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        //create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //update the previous created service account details by changing "name" and "description"
        var newTechUserName = $"UpdatedTechUserName_{now:s}";
        var newDescription = "This is an updated description";
        AdministrationEndpointHelper.UpdateServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString(),
            newTechUserName, newDescription);

        //check if the change of the serviceAccount got successfully saved
        var updatedServiceAccount =
            AdministrationEndpointHelper.GetServiceAccountDetailsById(newServiceAccount.ServiceAccountId.ToString());

        Assert.True(updatedServiceAccount.Name == newTechUserName);
        Assert.True(updatedServiceAccount.Description == newDescription);
    }

    //Scenario - Create a new service account and update the credentials
    [Fact]
    public async Task Scenario_HappyPathCreateServiceAccountAndUpdateCredentials()
    {
        await AdministrationEndpointHelper.GetOperatorToken();

        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //reset service account credentials
        var updatedServiceAccount =
            AdministrationEndpointHelper.ResetServiceAccountCredentialsById(
                newServiceAccount.ServiceAccountId.ToString());

        //check if the resetup of the credentials was successful
        Assert.True(newServiceAccount.Secret != updatedServiceAccount.Secret);

        //get a token with the new credentials to ensure that the reset was really successful
    }

    //Scenario - Create and delete a new service account
    [Fact]
    public async Task Scenario_HappyPathCreateAndDeleteServiceAccount()
    {
        await AdministrationEndpointHelper.GetOperatorToken();
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        // create a new service account
        var newServiceAccount =
            AdministrationEndpointHelper.CreateNewServiceAccount(techUserName, "This is a new test technical user");

        //  check if the new service account is available
        var existingServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        //delete the created service account
        AdministrationEndpointHelper.DeleteServiceAccount(newServiceAccount.ServiceAccountId.ToString());

        //check the endpoint, the deleted service account should not be available anymore
        var updatedServiceAccounts = AdministrationEndpointHelper.GetServiceAccounts();

        Assert.Empty(updatedServiceAccounts.Where(item => item.ServiceAccountId == newServiceAccount.ServiceAccountId));
    }

    private string? RetrieveTechUserToken(string client_id, string client_secret)
        // [Fact]
        // public void RetrieveTechUserToken()
    {
        // string client_id = "sa39", client_secret = "";
        var formData = new[]
        {
            new KeyValuePair<string, string>("client_secret", client_secret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "openid"),
            new KeyValuePair<string, string>("client_id", client_id),
        };

        var accessToken = Given()
            .ContentType("application/x-www-form-urlencoded")
            .FormData(formData)
            .When()
            .Post("https://centralidp.dev.demo.catena-x.net/auth/realms/CX-Central/protocol/openid-connect/token")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Body("$.access_token").ToString();

        return accessToken;
    }

    private static bool CheckTokenForAttribute(string jwtToken, string attribute)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
        return jwtSecurityToken.Payload.ContainsKey(attribute);
    }
}